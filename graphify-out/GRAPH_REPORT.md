# Graph Report - Probably Stolen  (2026-11-20)

## Corpus Check
- 30 files · ~14,158 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 6 file(s) not represented in the graph (top: .xml 5, (none) 1)

## Summary
- 194 nodes · 286 edges · 25 communities (6 shown, 19 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.95)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `2920bc6f`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SmartInventorySortMod.cs
- CustomerDemandHighlightMod
- SmartCategorySortController
- AugmentedDetectorMod.cs
- PreciseTemperatureControlMod.cs
- DisablePackShelvesPatch.Prefix
- json
- Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)
- Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%

## God Nodes (most connected - your core abstractions)
1. `SmartCategorySortController` - 36 edges
2. `CustomerDemandHighlightMod` - 9 edges
3. `LotteryForesightMod` - 9 edges
4. `ContainerConfig` - 8 edges
5. `SmartInventorySortMod` - 7 edges
6. `NamedInventory` - 6 edges
7. `Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)` - 6 edges
8. `AugmentedDetectorMod` - 5 edges
9. `PreciseTemperatureControlMod` - 5 edges
10. `TestMod` - 5 edges

## Surprising Connections (you probably didn't know these)
- `CustomerDemandHighlightMod` --references--> `GameItem`  [EXTRACTED]
  custom_mods/AlwaysHighlightCustomerDemand/CustomerDemandHighlightMod.cs →   _Bridges community 2 → community 1_
- `SmartCategorySortController` --references--> `ItemTagDef`  [EXTRACTED]
  custom_mods/SmartInventorySort/SmartInventorySortMod.cs → custom_mods/SmartInventorySort/SmartInventorySortMod.cs  _Bridges community 0 → community 2_

## Import Cycles
- None detected.

## Communities (25 total, 19 thin omitted)

### Community 0 - "SmartInventorySortMod.cs"
Cohesion: 0.07
Nodes (14): BlockCustomEventHandlerRaycastPatch, BlockInputActionHandlerUpdatePressPatch, BlockInputActionHandlerUpdateReleasePatch, BlockItemMouseDoubleClickHandlerClickActionPatch, BlockItemMouseDoubleClickHandlerDoubleClickActionPatch, BlockItemMouseDoubleClickHandlerOnEventPressPatch, BlockItemMouseDragHandlerStartDragPatch, BlockItemMouseHandlerOnEventPressPatch (+6 more)

### Community 1 - "CustomerDemandHighlightMod"
Cohesion: 0.08
Nodes (6): CustomerDemandHighlightMod, AugmentedDetectorMod, LotteryForesightMod, PreciseTemperatureControlMod, SmartInventorySortMod, TestMod

### Community 2 - "SmartCategorySortController"
Cohesion: 0.12
Nodes (3): ContainerConfig, NamedInventory, SmartCategorySortController

### Community 3 - "AugmentedDetectorMod.cs"
Cohesion: 0.10
Nodes (5): DialogBubbleColorAugPatch, DialogClientTextAugPatch, StoreClientIdleAugPatch, StoreClientOnLeftPatch, ScratcherQuickScratchPatch

### Community 4 - "PreciseTemperatureControlMod.cs"
Cohesion: 0.18
Nodes (3): HeatingPlateButtonsAlwaysShowPatch, HeatingPlateMinusPatch, HeatingPlatePlusPatch

### Community 7 - "Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)"
Cohesion: 0.18
Nodes (10): 1. 📦 Sắp Xếp & Phân Loại Kho Đa Hòm Thông Minh (`SmartInventorySort` v1.2.0), 2. 👁️ Máy Dò Dị Nhân & Ngụy Nhân (`AugmentedDetector`), 💡 Các cơ chế hiển thị trực quan 3 tầng:, 🛠️ Các Mod Bổ Trợ Khác Trong Bộ Sưu Tập, 💡 Các tính năng nổi bật:, 🌟 Danh Sách Các Mod Chính, 📜 Giấy Phép & Tuyên Bố Miễn Trừ Trách Nhiệm, 🔨 Hướng Dẫn Biên Dịch Mã Nguồn (Build from Source) (+2 more)

## Knowledge Gaps
- **19 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SmartCategorySortController` connect `SmartCategorySortController` to `SmartInventorySortMod.cs`, `CustomerDemandHighlightMod`?**
  _High betweenness centrality (0.231) - this node is a cross-community bridge._
- **Should `SmartInventorySortMod.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.07394957983193277 - nodes in this community are weakly interconnected._
- **Why does `SmartInventorySortMod` connect `CustomerDemandHighlightMod` to `SmartInventorySortMod.cs`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **Should `CustomerDemandHighlightMod` be split into smaller, more focused modules?**
  _Cohesion score 0.08064516129032258 - nodes in this community are weakly interconnected._
- **Why does `LotteryForesightMod` connect `CustomerDemandHighlightMod` to `SmartCategorySortController`, `AugmentedDetectorMod.cs`?**
  _High betweenness centrality (0.046) - this node is a cross-community bridge._
- **Should `SmartCategorySortController` be split into smaller, more focused modules?**
  _Cohesion score 0.11614401858304298 - nodes in this community are weakly interconnected._
- **Should `AugmentedDetectorMod.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.10144927536231885 - nodes in this community are weakly interconnected._