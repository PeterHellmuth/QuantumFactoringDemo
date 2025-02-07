# Base image with Python
FROM python:3.9-slim

# Install system dependencies
RUN apt-get update && apt-get install -y \
    build-essential \
    curl \
    git \
    libssl-dev \
    && rm -rf /var/lib/apt/lists/*

# Install the Azure Quantum Python SDK
RUN pip install --upgrade pip
RUN pip install flask qsharp azure-quantum

# Set the working directory
WORKDIR /app

# Copy all files in the current directory to /app in the container
COPY . /app

# Default command to start the Flask server
CMD ["python3", "shor_api.py"]