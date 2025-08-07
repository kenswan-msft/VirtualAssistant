# VirtualAssistant

Virtual Assistant based on local large language models (LLMs).

## Requirements

- .NET 9 or later
- A local LLM model (e.g., from [Hugging Face](https://huggingface.co/) or other sources)

## Installation

Navigate to `src/VirtualAssistant.Cli` and run:

```bash
dotnet pack
```

followed by

```bash
dotnet tool install --global --add-source ./bin/Tool/ VirtualAssistant.Cli
```

## Getting Started

### Load an LLM

To begin using the Virtual Assistant, you need to load a local LLM. You can do this by running:

```bash
jarvis model load -n <your-custom-name> -p <local-path-to-your-llm> -d <optional-description>
```

### Chat with the LLM

Once the model is loaded, you can start chatting with it:

```bash
jarvis
```

