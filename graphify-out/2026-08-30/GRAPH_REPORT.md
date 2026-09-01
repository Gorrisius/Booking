# Graph Report - ConferenceRoomBooking  (2026-08-30)

## Corpus Check
- Corpus is ~24,411 words - fits in a single context window. You may not need a graph.

## Summary
- 97 nodes · 80 edges · 22 communities (14 shown, 8 thin omitted)
- Extraction: 89% EXTRACTED · 11% INFERRED · 0% AMBIGUOUS · INFERRED: 9 edges (avg confidence: 0.88)
- Token cost: 20,569 input · 4,537 output

## Community Hubs (Navigation)
- ASP.NET Launch Configuration
- Graphify Pipeline Design
- Room Booking Requirements
- Graph Query Policies
- Graph Maintenance Automation
- Room Operations Service
- Room Domain Model
- Graph Traversal Workflow
- Incremental Graph Updates
- .NET Project Configuration
- Weather API Sample
- Semantic Extraction Model
- Media Transcription
- Graph Export Hub
- Repository Graph Merge
- Export Formats
- Token Benchmarking
- Build Output Artifacts

## God Nodes (most connected - your core abstractions)
1. `Meethods` - 6 edges
2. `Room` - 6 edges
3. `http` - 6 edges
4. `https` - 6 edges
5. `Graphify Knowledge Graph Pipeline` - 5 edges
6. `Incremental Re-Extraction` - 5 edges
7. `Graph Traversal` - 5 edges
8. `Conference Room Booking API` - 5 edges
9. `BFS and DFS Graph Traversal` - 4 edges
10. `WeatherForecast` - 3 edges

## Surprising Connections (you probably didn't know these)
- `Graphify Knowledge Graph Pipeline for Codex` --semantically_similar_to--> `Graphify Knowledge Graph Pipeline`  [INFERRED] [semantically similar]
  .codex/skills/graphify/SKILL.md → .claude/skills/graphify/SKILL.md
- `URL Ingestion for Codex` --semantically_similar_to--> `URL Ingestion`  [INFERRED] [semantically similar]
  .codex/skills/graphify/references/add-watch.md → .claude/skills/graphify/references/add-watch.md
- `Incremental Folder Watcher for Codex` --semantically_similar_to--> `Incremental Folder Watcher`  [INFERRED] [semantically similar]
  .codex/skills/graphify/references/add-watch.md → .claude/skills/graphify/references/add-watch.md
- `Graphify Project Policy` --semantically_similar_to--> `Graphify Project Policy`  [INFERRED] [semantically similar]
  AGENTS.md → CLAUDE.md
- `Native CLAUDE.md Integration` --conceptually_related_to--> `Graphify Project Policy`  [INFERRED]
  .codex/skills/graphify/references/hooks.md → CLAUDE.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Graphify Build and Refresh Modes** — claude_skills_graphify_skill_graphify_pipeline, claude_skills_graphify_references_update_incremental_reextraction, claude_skills_graphify_references_update_cluster_only [EXTRACTED 1.00]
- **Multimodal Content Extraction Flow** — claude_skills_graphify_skill_hybrid_extraction, claude_skills_graphify_references_transcribe_domain_guided_transcription, claude_skills_graphify_references_extraction_spec_semantic_extraction_contract [INFERRED 0.85]
- **Graph Maintenance Automation** — claude_skills_graphify_references_add_watch_folder_watcher, claude_skills_graphify_references_hooks_post_commit_graph_refresh, claude_skills_graphify_references_update_incremental_reextraction [INFERRED 0.85]
- **Graph Query Modes** — _codex_skills_graphify_references_query_breadth_first_search, _codex_skills_graphify_references_query_depth_first_search, _codex_skills_graphify_references_query_graph_traversal [EXTRACTED 1.00]

## Communities (22 total, 8 thin omitted)

### Community 0 - "ASP.NET Launch Configuration"
Cohesion: 0.13
Nodes (15): ASPNETCORE_ENVIRONMENT, applicationUrl, commandName, dotnetRunMessages, environmentVariables, launchBrowser, applicationUrl, commandName (+7 more)

### Community 1 - "Graphify Pipeline Design"
Cohesion: 0.18
Nodes (12): Graphify Invocation Directive, Confidence Audit Trail, Semantic Extraction Contract, Cross-Repository Graph Merge, Repository Origin Attribution, Native CLAUDE.md Integration, Domain-Guided Whisper Transcription, Transcript-to-Document Bridge (+4 more)

### Community 2 - "Room Booking Requirements"
Cohesion: 0.22
Nodes (9): Clean Code, Conference Room, Conference Room Booking API, Rental Price Calculation, Robert Cecil Martin, Room Availability Search, Room Booking, Swagger API Documentation (+1 more)

### Community 3 - "Graph Query Policies"
Cohesion: 0.25
Nodes (8): Native CLAUDE.md Integration, Breadth-First Search, Constrained Query Expansion, Depth-First Search, Graph Traversal, Saved Query Feedback Loop, Graphify Project Policy, Graphify Project Policy

### Community 4 - "Graph Maintenance Automation"
Cohesion: 0.29
Nodes (8): Incremental Folder Watcher, URL Ingestion, Post-Commit Graph Refresh, Cluster-Only Refresh, Incremental Re-Extraction, Replace-on-Re-Extract Merge, Incremental Folder Watcher for Codex, URL Ingestion for Codex

### Community 6 - "Room Domain Model"
Cohesion: 0.33
Nodes (5): Room, Capacity, Name, Options, Value

### Community 7 - "Graph Traversal Workflow"
Cohesion: 0.40
Nodes (5): MCP Graph Server, Constrained Query Expansion, BFS and DFS Graph Traversal, Work Memory Feedback Loop, Existing Graph Fast Path

### Community 8 - "Incremental Graph Updates"
Cohesion: 0.50
Nodes (4): Graphify Post-Commit Hook, Cluster-Only Refresh, Incremental Graph Update, Incremental Manifest Management

### Community 9 - ".NET Project Configuration"
Cohesion: 0.50
Nodes (3): net10.0, Microsoft.AspNetCore.OpenApi (10.0.11), Microsoft.NET.Sdk.Web

### Community 10 - "Weather API Sample"
Cohesion: 0.50
Nodes (3): DateOnly, WeatherForecast, TemperatureF

## Knowledge Gaps
- **43 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.NET.Sdk.Web`, `Name`, `Capacity` (+38 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What connects `net10.0`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.NET.Sdk.Web` to the rest of the system?**
  _43 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ASP.NET Launch Configuration` be split into smaller, more focused modules?**
  _Cohesion score 0.13333333333333333 - nodes in this community are weakly interconnected._