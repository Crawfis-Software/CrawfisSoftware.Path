# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- In this codebase, think of a loop 'cross section' as a horizontal bridge crossing over a vertical path; multiple such cross sections can exist, but edges must not repeat. For loops, ensure there are no duplicate edges (undirected edge reuse). Vertex crossings are allowed; horizontal paths (e.g., A->B->C) can intersect vertical paths (e.g., E->C->D) at shared vertices without violating the rule.
- In PathTests, keep strict undirected duplicate-edge validation for both paths and loops (including the closing end->start edge). Loop/path fixtures that reuse edges are expected to fail; do not relax validation to make them pass.