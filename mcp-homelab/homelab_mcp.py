import subprocess
import os
import httpx
from mcp.server.fastmcp import FastMCP

# 1. Initialize FastMCP server
mcp = FastMCP("Homelab-Admin-Server")

# Configuration Variables (Adjust as needed)
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://127.0.0.1:11434")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "phi3") # Fast model for summaries

# --- Helper Function: Call Ollama for Summaries ---
async def summarize_with_ollama(prompt_text, raw_data):
    full_prompt = f"{prompt_text}\n\nRAW DATA:\n{raw_data}"
    payload = {
        "model": OLLAMA_MODEL,
        "prompt": full_prompt,
        "stream": False
    }
    async with httpx.AsyncClient(timeout=30.0) as client:
        response = await client.post(f"{OLLAMA_URL}/api/generate", json=payload)
        return response.json().get("response", "Error generating summary.")

# --- TOOL 1: Health Check ---
@mcp.tool()
def health_check():
    """Confirms server status and enabled features."""
    return {
        "status": "online",
        "gpu_support": "ROCm/Polaris 570",
        "docker_tools": True,
        "allowed_services": ["docker", "ollama", "nginx"]
    }

# --- TOOL 2: Disk Usage (Raw & Summary) ---
@mcp.tool()
def get_disk_usage():
    """Returns raw df -h output."""
    result = subprocess.check_output(["df", "-h"], text=True)
    return result

@mcp.tool()
async def summarize_disk():
    """Runs df -h and returns an AI-interpreted summary."""
    raw = get_disk_usage()
    return await summarize_with_ollama("Summarize this disk usage. Highlight any partitions above 80%.", raw)

# --- TOOL 3: Memory Usage (Raw & Summary) ---
@mcp.tool()
def get_memory_usage():
    """Returns raw free -h output."""
    result = subprocess.check_output(["free", "-h"], text=True)
    return result

@mcp.tool()
async def summarize_memory():
    """Runs free -h and returns an AI-interpreted summary."""
    raw = get_memory_usage()
    return await summarize_with_ollama("Analyze this memory usage. Is there enough swap? Are we OOM-risky?", raw)

# --- TOOL 4: Docker Status ---
@mcp.tool()
def list_docker_containers():
    """Lists running Docker containers."""
    try:
        result = subprocess.check_output(["docker", "ps", "--format", "table {{.Names}}\t{{.Status}}\t{{.Ports}}"], text=True)
        return result
    except Exception as e:
        return f"Docker Error: {str(e)}"

@mcp.tool()
async def summarize_docker():
    """Returns an AI summary of currently running Docker services."""
    raw = list_docker_containers()
    return await summarize_with_ollama("Summarize which services are healthy and which ports are exposed.", raw)

if __name__ == "__main__":
    mcp.run()
