Technical Drawing Data Extraction System

Sponsor: Northeast Precast

Course: SWE I - Sponsor Project

---

## Overview
This project builds a system to automatically extract data from precast concrete shop tickets(PDFS).
Currently, engineers enter this information manually into spreadsheets, which is time-consuming and error prone.
Our system parses PDF shop ticketsand returns data object for use in reporting and analytics.

---

## Project Goals 
- Automate data extraction from shop tickets.
- Store extracted information in structured formats 
- Reduce manual engineering work.
- Enable analytics and reporting.
- Explore advanced methods (ML/AI methods)

---

## Extracted Properties
The system extracts the following from each PDF:

- Metadata: Number of pages, file name, page labels.
- Identifiers: Project Number, project name, piece mark, control numbers.
- Details: Pieces required, weight, design number.
- Geomatry: Form view rectangle, section view rectangle

---

## Technology Stack
- Language: TBD
- Libraries: TBD
- Version Control: https://github.com/Critchfieldz21/Blobfish
- Task Management: Trello Board 

---

## Scrum Workflow
5 Sprints in total.

- Product Backlog: Organized in Trello with PBIs.
- Spring Backlog: Selected PBI's for each sprint.
- In Progress / Review / Done: Trello workflow lists
- Scrum Roles:
    - Product owner: Duwon Ham
    - Scrum Master: Keven Guzman
    - Development Team: Chiemeka Chukwueke, Zachary Critchfield, Lance Ilagan, Mehakjot singh
      

## Sprint Goals
1. Sprint 1: IDE Setup, Metadata extraction
2. Sprint 2: Title block data extraction.
3. Sprint 3: Geometry (form/section rectangles).
4. Sprint 4: Data structuring & output.
5. Sprint 5: Advanced feaetures, UI, final demo.

---

## How to run the project

-----TBD------

---
## bash 
git clone https://github.com/Critchfieldz21/Blobfish.git

cd Blobfish
