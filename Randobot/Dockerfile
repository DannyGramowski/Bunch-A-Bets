# Use the official lightweight Python image
FROM python:3.12-slim

# Set environment variables to reduce output noise and avoid .pyc files
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

# Set the working directory in the container
WORKDIR /app

# Copy the Python bot script and any other required files
COPY . /app

# Install dependencies (if any)
RUN pip install --no-cache-dir -r requirements.txt

# Run the bot
# host.docker.internal will redirect to 127.0.0.1 on the host machine when running through Docker.
# To test this Dockerfile, change this to the server's IP address.
CMD ["python", "main.py", "host.docker.internal"]