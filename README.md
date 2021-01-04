# OpenALPR Webhook Processor

This service accepts license plate webhooks from the OpenALPR web server. The webhook data (license plate number, make/model, year) is then set as text on the configured IP camera video overlay.

## Quick Start
The container needs 1 port for incoming connections, and an appsettings.json volume to configure the cameras to update. Check the `ConfigurationExamples` folder for guidance.

## Windows Configuration
Fill out the `appsettings.json` file with camera details then start the application.

**command line**

    dotnet ./OpenAlprWebhookProcessor.dll

## Docker Configuration
**docker cli**

    docker run -d \
    --name=openalprwebhookprocessor \
    --net=bridge \
    -v /app/appsettings.json:/app/appsettings.json \
	-v /app/processor.db:/app/processor.db \
    -p 3859:80 \
    mlapaglia/openalprwebhookprocessor
    
## Docker Hub
https://hub.docker.com/repository/docker/mlapaglia/openalprwebhookprocessor
