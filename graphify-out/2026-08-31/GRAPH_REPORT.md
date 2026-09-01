# Graph Report - ConferenceRoomBooking  (2026-08-31)

## Corpus Check
- 50 files · ~31,072 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 131 nodes · 115 edges · 23 communities (15 shown, 8 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 9 edges (avg confidence: 0.88)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- ASP.NET Launch Configuration
- Graphify Pipeline Design
- Room Booking Requirements
- Graph Query Policies
- Graph Maintenance Automation
- Meethods
- Room Domain Model
- Graph Traversal Workflow
- Incremental Graph Updates
- ConferenceRoomBooking.csproj
- ConferenceRoomBooking.Data
- Semantic Extraction Model
- Media Transcription
- Graph Export Hub
- Repository Graph Merge
- Export Formats
- Token Benchmarking
- Build Output Artifacts
- Room

## God Nodes (most connected - your core abstractions)
1. `SQLite basics for ConferenceRoomBooking` - 17 edges
2. `Room` - 10 edges
3. `Booking` - 8 edges
4. `Meethods` - 6 edges
5. `http` - 6 edges
6. `https` - 6 edges
7. `Graphify Knowledge Graph Pipeline` - 5 edges
8. `Conference Room Booking API` - 5 edges
9. `Graph Traversal` - 5 edges
10. `Incremental Re-Extraction` - 5 edges

## Surprising Connections (you probably didn't know these)
- `Graphify Knowledge Graph Pipeline for Codex` --semantically_similar_to--> `Graphify Knowledge Graph Pipeline`  [INFERRED] [semantically similar]
  .codex/skills/graphify/SKILL.md → .claude/skills/graphify/SKILL.md
- `Graphify Project Policy` --semantically_similar_to--> `Graphify Project Policy`  [INFERRED] [semantically similar]
  AGENTS.md → CLAUDE.md
- `Incremental Folder Watcher for Codex` --semantically_similar_to--> `Incremental Folder Watcher`  [INFERRED] [semantically similar]
  .codex/skills/graphify/references/add-watch.md → .claude/skills/graphify/references/add-watch.md
- `URL Ingestion for Codex` --semantically_similar_to--> `URL Ingestion`  [INFERRED] [semantically similar]
  .codex/skills/graphify/references/add-watch.md → .claude/skills/graphify/references/add-watch.md
- `Native CLAUDE.md Integration` --conceptually_related_to--> `Graphify Project Policy`  [INFERRED]
  .codex/skills/graphify/references/hooks.md → CLAUDE.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Graph Query Modes** — _codex_skills_graphify_references_query_breadth_first_search, _codex_skills_graphify_references_query_depth_first_search, _codex_skills_graphify_references_query_graph_traversal [EXTRACTED 1.00]
- **Graphify Build and Refresh Modes** — claude_skills_graphify_skill_graphify_pipeline, claude_skills_graphify_references_update_incremental_reextraction, claude_skills_graphify_references_update_cluster_only [EXTRACTED 1.00]
- **Multimodal Content Extraction Flow** — claude_skills_graphify_skill_hybrid_extraction, claude_skills_graphify_references_transcribe_domain_guided_transcription, claude_skills_graphify_references_extraction_spec_semantic_extraction_contract [INFERRED 0.85]
- **Graph Maintenance Automation** — claude_skills_graphify_references_add_watch_folder_watcher, claude_skills_graphify_references_hooks_post_commit_graph_refresh, claude_skills_graphify_references_update_incremental_reextraction [INFERRED 0.85]

## Communities (23 total, 8 thin omitted)

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
Cohesion: 0.11
Nodes (17): 10. Useful development commands, 1. Install EF Core's SQLite provider, 2. Make the room entity persistable, 3. Create a DbContext, 4. Add the connection string, 5. Register the context in Program.cs, 6. Create the schema with migrations, 7. Basic EF Core operations (+9 more)

### Community 7 - "Graph Traversal Workflow"
Cohesion: 0.40
Nodes (5): MCP Graph Server, Constrained Query Expansion, BFS and DFS Graph Traversal, Work Memory Feedback Loop, Existing Graph Fast Path

### Community 8 - "Incremental Graph Updates"
Cohesion: 0.50
Nodes (4): Graphify Post-Commit Hook, Cluster-Only Refresh, Incremental Graph Update, Incremental Manifest Management

### Community 9 - "ConferenceRoomBooking.csproj"
Cohesion: 0.29
Nodes (6): net10.0, Microsoft.AspNetCore.OpenApi (10.0.11), Microsoft.Data.Sqlite (10.0.11), Microsoft.EntityFrameworkCore.Design (10.0.11), Microsoft.EntityFrameworkCore.Sqlite (10.0.11), Microsoft.NET.Sdk.Web

### Community 10 - "ConferenceRoomBooking.Data"
Cohesion: 0.33
Nodes (3): ConferenceRoomBooking.Data, DatabaseInitializer, Task

### Community 22 - "Room"
Cohesion: 0.12
Nodes (16): ConferenceRoomBooking.Models, DateTimeOffset, List, Booking, EndAt, Id, RoomId, StartAt (+8 more)

## Knowledge Gaps
- **68 isolated node(s):** `net10.0`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.Data.Sqlite (10.0.11)`, `Microsoft.EntityFrameworkCore.Design (10.0.11)`, `Microsoft.EntityFrameworkCore.Sqlite (10.0.11)` (+63 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **8 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Room` connect `Room` to `Meethods`?**
  _High betweenness centrality (0.023) - this node is a cross-community bridge._
- **What connects `net10.0`, `Microsoft.AspNetCore.OpenApi (10.0.11)`, `Microsoft.Data.Sqlite (10.0.11)` to the rest of the system?**
  _68 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ASP.NET Launch Configuration` be split into smaller, more focused modules?**
  _Cohesion score 0.13333333333333333 - nodes in this community are weakly interconnected._
- **Should `Room Domain Model` be split into smaller, more focused modules?**
  _Cohesion score 0.1111111111111111 - nodes in this community are weakly interconnected._
- **Should `Room` be split into smaller, more focused modules?**
  _Cohesion score 0.11764705882352941 - nodes in this community are weakly interconnected._