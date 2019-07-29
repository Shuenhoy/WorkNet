FROM mcr.microsoft.com/dotnet/core/sdk:3.0
WORKDIR /app
RUN useradd -ms /bin/bash newuser

USER newuser

ENV PATH="/home/newuser/.dotnet/tools:$PATH"
EXPOSE 5000 5001
RUN /bin/sh