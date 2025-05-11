# 🧠 Unity ML-Agents CLI Commands Cheat Sheet

This document lists commonly used `mlagents-learn` CLI commands and options, along with explanations.

Useful : mlagents-learn results/Prey001/configuration.yaml --run-id=Prey001 --force

## ✅ Basic Training Command

mlagents-learn --run-id=Agent

## 🔰 Using a config file

mlagents-learn config/trainer_config.yaml --run-id=Agent

- **Purpose**: Starts training with the given config.
- **`--run-id`**: Sets a unique session ID used for saving models and logs.

## 🧪 Run in Unity Editor

mlagents-learn config/trainer_config.yaml --run-id=Agent --env-args --no-graphics

- Unity must be in play mode with an Agent scene open.
- **No build is required.**

## 🏗️ Train with Built Game

mlagents-learn config/trainer_config.yaml --run-id=Agent --env=builds/MyGame.exe --no-graphics

- **`--env`**: Points to your Unity executable.
- Works for training without Unity Editor.

## 📈 Use TensorBoard to Monitor

tensorboard --logdir results

- Shows reward, loss, entropy, etc.
- Run in browser at: `http://localhost:6006/`

## 🧠 Run Inference (No Training)

mlagents-learn config/trainer_config.yaml --run-id=Agent --inference

- Loads trained model and runs in inference mode (no updates to weights).

## 🕹️ Resume Training from Checkpoint

mlagents-learn config/trainer_config.yaml --run-id=Agent --resume

- Picks up from the latest saved checkpoint for the run ID.

## 🧼 Force override over an existing Run

mlagents-learn config.yaml --run-id=Agent --force

- Overrides a previous run erasing all data

## 📚 Use Curriculum Learning

mlagents-learn config/trainer_config.yaml --run-id=Agent --curriculum=config/curriculum/

- **`--curriculum`**: Path to folder containing curriculum JSON files.

## 🔄 Train Multiple Agents

mlagents-learn config/trainer_config.yaml --run-id=MultiAgent --env=builds/Game.exe --num-envs=4 --no-graphics

- Spawns multiple environments in parallel to speed up training.
- **Tip**: Use this only if your game supports multiple instances cleanly.

## 🧊 Headless Mode on Servers (Linux)

xvfb-run -a mlagents-learn config/trainer_config.yaml --run-id=ServerRun --env=Game.x86_64 --no-graphics

- Runs without graphical display using `xvfb`.

## 🔁 Transfer Learning (Load from Existing)

mlagents-learn config/trainer_config.yaml --run-id=AgentFineTune --initialize-from=Agent

- Uses weights from an earlier run ID to fine-tune further.

## 🔬 Debug Training with Verbose Logging

mlagents-learn config/trainer_config.yaml --run-id=DebugRun --env=builds/Game.exe --debug

- Logs internal decisions, value predictions, etc.

## 🧼 Clean Old Checkpoints (Optional)

rm -rf results/Agent

- Clears previous model and stats for fresh training.

## 📦 Output Location

- Models, summaries, checkpoints: `results/{run-id}/`
- Final model: `results/{run-id}/BehaviorName.onnx`

Made for Unity ML-Agents Toolkit users.
