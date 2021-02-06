# OpenALPR Webhook Processor

This service accepts license plate webhooks from the OpenALPR web server. The webhook data (license plate number, make/model, year) is then set as text on the configured IP camera video overlay.

## Screenshots
### Plates view
![image](https://user-images.githubusercontent.com/4184746/104876013-ba0c6000-5924-11eb-9035-c9d5ab481959.png)

### Camera Configuration
![image](https://user-images.githubusercontent.com/4184746/104876061-d9a38880-5924-11eb-93a0-5600162f7477.png)

### Agent Configuration
![image](https://user-images.githubusercontent.com/4184746/104876130-035caf80-5925-11eb-8b24-cd47ef551295.png)

## Quick Start
**command line**

    dotnet ./OpenAlprWebhookProcessor.dll

## Docker Configuration
The container needs 1 port for incoming connections and a volume to mount the databses for plate/settings storage

**docker cli**

    docker run -d \
    --name=openalprwebhookprocessor \
    --net=bridge \
    -v /host/path/:/app/config/ \
    -p 3859:80 \
    mlapaglia/openalprwebhookprocessor
    
## Docker Hub
https://hub.docker.com/repository/docker/mlapaglia/openalprwebhookprocessor
