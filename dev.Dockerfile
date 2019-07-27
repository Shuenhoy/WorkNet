FROM mcr.microsoft.com/dotnet/core/sdk:3.0
WORKDIR /app
RUN useradd -ms /bin/bash newuser
USER newuser
EXPOSE 5000 5001
RUN /bin/sh