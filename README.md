# 🌿 Ecosystem-MLAgents

A Unity ML-Agents simulation modeling **autonomous prey-predator dynamics** using reinforcement learning, curriculum learning, and evolutionary principles.

This multi-agent environment explores how intelligent virtual organisms interact, adapt, and evolve in a shared ecosystem governed by resource scarcity, survival pressures, and reproduction incentives.

---

## 🧬 Project Overview

This simulation aims to demonstrate emergent behavior and evolutionary strategies within a digital ecosystem using Unity + ML-Agents. It features:

### ✅ Completed: **Prey Behavior Simulation**

Prey agents are trained to:
- Forage for **food** and **water** based on internal hunger/thirst states
- Seek out **mates** for reproduction under appropriate conditions
- Prioritize actions dynamically through a **reward-based decision system**

**Curriculum Learning Pipeline:**
1. Stage 1 – **Foraging + Thirst Management**
2. Stage 2 – **Full Cycle**: Foraging + Thirst + Reproduction
3. Stage 3 – **Full predator Cycle**: Foraging + Thirst + Reproduction + Hunting

---

## 🧠 AI & Learning System

- **Reinforcement Learning**: Multi-agent PPO policies guide each agent’s decision-making.
- **Event-Based Feedback**: Environment responds to actions (e.g., food consumed, mate proximity).
- **Behavior Prioritization**: Agents learn to manage conflicting internal drives over time.

---

## 🧬 Population Genetics & Evolution

Each agent maintains a lightweight genome:
- Traits include: `visionRange`, `speed`, `stamina`, `maxLifetime`
- Reproduction transfers traits with **genetic mutation and crossover**
- Over generations, agent populations adapt to environment dynamics
- Supports tracking of **evolutionary fitness trends**

---

## 🌎 Environment Dynamics

- **Resource Spawning**: Food/water patches appear at random zones with limited density
- **Temporal Pressure**: Agents age and die; only those who reproduce sustain the population
- **Density Regulation**: Natural fluctuations in agent count based on survival efficiency

---

## 📌 Status

- ✅ Prey agents trained and stable through generations  
- 🔄 Predator integration underway  
- 🚀 Planned: Full predator-prey simulation with ecosystem balancing

---

## 🔄 In Progress: Predator Integration

Currently developing a new predator species with:
- **Hunting behavior** based on prey visibility
- **Stealth & ambush mechanics**
- **Prey targeting heuristics** (e.g., selecting weakest prey)
- Long-term goal: **Emergent arms race** between prey evasiveness and predator efficiency

---

## 📊 Episode Logging & Agent Statistics

To enable deeper analysis and debugging, the simulation includes a **custom statistics logging system** that captures detailed metrics for each agent and episode. This system tracks:

- **Agent Lifetime**: Total time steps survived
- **Cause of Death**: Starvation, dehydration, aging, or predation (when predators are implemented)
- **Trait Summary**: Each agent's genome (vision, speed, stamina, lifespan)
- **Reproduction Events**: Parent-offspring lineage tracking
- **Resource Consumption**: Total food and water consumed per agent
- **Global Episode Data**: Population dynamics, total deaths/births, survival rates, and fitness averages

All statistics are written to structured logs (e.g., JSON or CSV), allowing for:
- Episode-wise postmortem analysis
- Visualization of generational improvements
- Balancing of evolutionary pressure and ecosystem sustainability

This robust logging infrastructure supports the long-term goal of **quantifying emergent behavior and evolutionary success** across simulated generations.

---

## 🧪 Research Potential

This simulation can be extended or used for:
- Multi-agent learning studies
- Emergence of cooperative/competitive dynamics
- Evolutionary algorithm experimentation
- Behavioral ecology visualizations
