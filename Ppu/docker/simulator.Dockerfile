FROM python:3.12-slim

WORKDIR /app

COPY pstg/pyproject.toml ./
COPY pstg/README.md ./
COPY pstg/src ./src

RUN pip install --no-cache-dir .
RUN pip install --no-cache-dir --force-reinstall pymodbus==3.11.3

EXPOSE 1502

ENTRYPOINT ["python", "-m", "pstg.simulator.server"]
