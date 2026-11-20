# Graph Report - Probably Stolen  (2026-11-20)

## Corpus Check
- 29 files · ~14,255 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 6 file(s) not represented in the graph (top: .xml 5, (none) 1)

## Summary
- 196 nodes · 293 edges · 25 communities (7 shown, 18 thin omitted)
- Extraction: 100% EXTRACTED · 0% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.95)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e119bc12`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SmartInventorySortMod.cs
- CustomerDemandHighlightMod
- SmartCategorySortController
- AugmentedDetectorMod.cs
- PreciseTemperatureControlMod.cs
- GameItem
- json
- Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)
- Bàn Giao Tiến Độ Kỹ Thuật (Checkpoint) - Hoàn Tất 100%
- SmartCategorySortController.GetAvailableInventories

## God Nodes (most connected - your core abstractions)
1. `SmartCategorySortController` - 38 edges
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
- `CustomerDemandHighlightMod` --implements--> `IMod`  [EXTRACTED]
  custom_mods/AlwaysHighlightCustomerDemand/CustomerDemandHighlightMod.cs →   _Bridges community 1 → community 5_
- `NamedInventory` --references--> `ContainerConfig`  [EXTRACTED]
  custom_mods/SmartInventorySort/SmartInventorySortMod.cs → custom_mods/SmartInventorySort/SmartInventorySortMod.cs  _Bridges community 2 → community 17_

## Import Cycles
- None detected.

## Communities (25 total, 18 thin omitted)

### Community 0 - "SmartInventorySortMod.cs"
Cohesion: 0.08
Nodes (13): BlockCustomEventHandlerRaycastPatch, BlockInputActionHandlerUpdatePressPatch, BlockInputActionHandlerUpdateReleasePatch, BlockItemMouseDoubleClickHandlerClickActionPatch, BlockItemMouseDoubleClickHandlerDoubleClickActionPatch, BlockItemMouseDoubleClickHandlerOnEventPressPatch, BlockItemMouseDragHandlerStartDragPatch, BlockItemMouseHandlerOnEventPressPatch (+5 more)

### Community 1 - "CustomerDemandHighlightMod"
Cohesion: 0.11
Nodes (4): AugmentedDetectorMod, PreciseTemperatureControlMod, SmartInventorySortMod, TestMod

### Community 2 - "SmartCategorySortController"
Cohesion: 0.12
Nodes (3): ContainerConfig, ItemTagDef, SmartCategorySortController

### Community 3 - "AugmentedDetectorMod.cs"
Cohesion: 0.10
Nodes (5): DialogBubbleColorAugPatch, DialogClientTextAugPatch, StoreClientIdleAugPatch, StoreClientOnLeftPatch, ScratcherQuickScratchPatch

### Community 4 - "PreciseTemperatureControlMod.cs"
Cohesion: 0.18
Nodes (3): HeatingPlateButtonsAlwaysShowPatch, HeatingPlateMinusPatch, HeatingPlatePlusPatch

### Community 5 - "GameItem"
Cohesion: 0.15
Nodes (3): CustomerDemandHighlightMod, LotteryForesightMod, DisablePackShelvesPatch

### Community 7 - "Probably Stolen - Native IMod Collection (Bộ Mod Tối Ưu & Tiện Ích Thuần Việt)"
Cohesion: 0.18
Nodes (10): 1. 📦 Sắp Xếp & Phân Loại Kho Đa Hòm Thông Minh (`SmartInventorySort` v1.2.0), 2. 👁️ Máy Dò Dị Nhân & Ngụy Nhân (`AugmentedDetector`), 💡 Các cơ chế hiển thị trực quan 3 tầng:, 🛠️ Các Mod Bổ Trợ Khác Trong Bộ Sưu Tập, 💡 Các tính năng nổi bật:, 🌟 Danh Sách Các Mod Chính, 📜 Giấy Phép & Tuyên Bố Miễn Trừ Trách Nhiệm, 🔨 Hướng Dẫn Biên Dịch Mã Nguồn (Build from Source) (+2 more)

## Knowledge Gaps
- **18 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SmartCategorySortController` connect `SmartCategorySortController` to `SmartInventorySortMod.cs`, `SmartCategorySortController.GetAvailableInventories`, `GameItem`, `CustomerDemandHighlightMod`?**
  _High betweenness centrality (0.246) - this node is a cross-community bridge._
- **Should `SmartInventorySortMod.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.07954545454545454 - nodes in this community are weakly interconnected._
- **Why does `SmartInventorySortMod` connect `CustomerDemandHighlightMod` to `SmartInventorySortMod.cs`?**
  _High betweenness centrality (0.063) - this node is a cross-community bridge._
- **Should `CustomerDemandHighlightMod` be split into smaller, more focused modules?**
  _Cohesion score 0.10952380952380952 - nodes in this community are weakly interconnected._
- **Why does `LotteryForesightMod` connect `GameItem` to `CustomerDemandHighlightMod`, `AugmentedDetectorMod.cs`?**
  _High betweenness centrality (0.047) - this node is a cross-community bridge._
- **Should `SmartCategorySortController` be split into smaller, more focused modules?**
  _Cohesion score 0.11942959001782531 - nodes in this community are weakly interconnected._
- **Should `AugmentedDetectorMod.cs` be split into smaller, more focused modules?**
  _Cohesion score 0.10144927536231885 - nodes in this community are weakly interconnected._