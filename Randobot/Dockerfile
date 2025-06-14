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
CMD ["python", "main.py"]