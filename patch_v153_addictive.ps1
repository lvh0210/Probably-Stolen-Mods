$file = "D:\VibeCode\Probably Stolen\custom_mods\SmartInventorySort\SmartInventorySortMod.cs"
$content = [System.IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)

# =========================================================
# 1. Thêm tag ADDICTIVE vào AllTags (sau CONTRABAND line)
# =========================================================
$old1 = 'new ItemTagDef("CONTRABAND", "☠️ Hàng Cấm & Độc Dược", "Hàng Cấm"),'
$new1 = 'new ItemTagDef("CONTRABAND", "☠️ Hàng Cấm & Độc Dược", "Hàng Cấm"),
        new ItemTagDef("ADDICTIVE", "💉 Chất Gây Nghiện (Ma Túy/Chất Cấm)", "Gây Nghiện"),'
if ($content.Contains($old1)) { $content = $content.Replace($old1, $new1); Write-Host "PATCH 1 OK" } else { Write-Host "PATCH 1 NOT FOUND" }

# =========================================================
# 2. Dòng 996 trong GetItemTagIds: Tách NARCOTIC ra khỏi CONTRABAND
# =========================================================
$old2 = '        if (item.IsTag("CONTRABAND") || item.IsTag("CONTRABAND_ITEM_TAG") || item.IsTag("NARCOTIC") || item.IsTag("POISON") || item.IsTag("RESTRICTED_1") || item.IsTag("RESTRICTED_2") || item.IsTag("RESTRICTED_3") || item.IsTag("TRAFFICKING")) tags.Add("CONTRABAND");'
$new2 = @'
        // Hàng Cấm chính thức: vũ khí lậu, hàng cấm xuất nhập khẩu, độc chất thực sự
        if (item.IsTag("CONTRABAND") || item.IsTag("CONTRABAND_ITEM_TAG") || item.IsTag("RESTRICTED_1") || item.IsTag("RESTRICTED_2") || item.IsTag("RESTRICTED_3") || item.IsTag("TRAFFICKING") || item.IsTag("POISON")) tags.Add("CONTRABAND");
        // Chất gây nghiện: NARCOTIC có thể là rượu/thuốc lá gây nghiện hợp pháp — xử lý riêng bên dưới (Master Safety Shield)
        if (item.IsTag("NARCOTIC")) tags.Add("ADDICTIVE");
'@
if ($content.Contains($old2)) { $content = $content.Replace($old2, $new2); Write-Host "PATCH 2 OK" } else { Write-Host "PATCH 2 NOT FOUND" }

# =========================================================
# 3. AddMatchingTagIds: Tách NARCOTIC ra khỏi CONTRABAND
# =========================================================
$old3 = '        if (t == "CONTRABAND" || t == "RESTRICTED_1" || t == "RESTRICTED_2" || t == "RESTRICTED_3" || t == "NARCOTIC" || t == "POISON") { tags.Add("CONTRABAND"); return; }'
$new3 = @'
        if (t == "CONTRABAND" || t == "RESTRICTED_1" || t == "RESTRICTED_2" || t == "RESTRICTED_3" || t == "POISON" || t == "TRAFFICKING") { tags.Add("CONTRABAND"); return; }
        if (t == "NARCOTIC" || t == "ADDICTIVE") { tags.Add("ADDICTIVE"); return; }
'@
if ($content.Contains($old3)) { $content = $content.Replace($old3, $new3); Write-Host "PATCH 3 OK" } else { Write-Host "PATCH 3 NOT FOUND" }

# =========================================================
# 4. Identifier matching: Tách narcotic-based identifiers
# =========================================================
$old4 = '        // Hàng Cấm & Độc Dược: Chỉ nhận chất cấm thực sự
        if (id.Contains("contraband") || id.Contains("narcotic") || id.Contains("fentanyl") || id.Contains("zyanide") || id.Contains("fanta_pill") || id.Contains("dream_dust") || id.Contains("nightmare_dust")) tags.Add("CONTRABAND");'
$new4 = @'
        // Hàng Cấm: Các chất bị pháp luật cấm hoàn toàn (vũ khí lậu, độc dược, chất cấm phân phối)
        if (id.Contains("contraband") || id.Contains("fentanyl") || id.Contains("zyanide") || id.Contains("fanta_pill") || id.Contains("dream_dust") || id.Contains("nightmare_dust")) tags.Add("CONTRABAND");
        // Chất Gây Nghiện: Ma túy, chất kích thần kinh không phân loại rõ ràng là hàng cấm hay y tế
        if (id.Contains("narcotic") || id.Contains("stimulant") || id.Contains("stim_pack") || id.Contains("rush_pill") || id.Contains("high_pill") || id.Contains("euphoria")) tags.Add("ADDICTIVE");
'@
if ($content.Contains($old4)) { $content = $content.Replace($old4, $new4); Write-Host "PATCH 4 OK" } else { Write-Host "PATCH 4 NOT FOUND" }

# =========================================================
# 5. Master Safety Shield: Nếu có ALCOHOL hoặc TOBACCO → chuyển ADDICTIVE hợp lệ, xóa CONTRABAND nếu không phải hàng cấm chính thức
# Thêm sau block (c) MATERIAL conflict resolver
# =========================================================
$old5 = '        if (tags.Count == 0) tags.Add("GENERAL");
        return tags;'
$new5 = @'
        // d) Phân loại ADDICTIVE hợp lệ vs CONTRABAND thực sự
        // Nếu item có ADDICTIVE (do NARCOTIC tag) nhưng lại cũng thuộc ALCOHOL hoặc TOBACCO
        // → Đây là chất gây nghiện hợp pháp (rượu, thuốc lá, chất kích thích thông thường) → XÓA khỏi CONTRABAND
        if (tags.Contains("ADDICTIVE"))
        {
            bool isOfficiallyBanned = item.IsTag("CONTRABAND") || item.IsTag("CONTRABAND_ITEM_TAG") ||
                                      item.IsTag("RESTRICTED_1") || item.IsTag("RESTRICTED_2") || item.IsTag("RESTRICTED_3") ||
                                      item.IsTag("TRAFFICKING");
            bool isLegalSubstance = tags.Contains("ALCOHOL") || tags.Contains("TOBACCO") || tags.Contains("MEDICAL") || tags.Contains("FOOD");
            if (!isOfficiallyBanned)
            {
                // Hợp pháp: giữ ADDICTIVE, xóa CONTRABAND nhầm
                tags.Remove("CONTRABAND");
                if (isLegalSubstance)
                {
                    // Rượu/thuốc lá gây nghiện nhưng hợp pháp → không cần đánh thêm ADDICTIVE riêng
                    tags.Remove("ADDICTIVE");
                }
            }
            else
            {
                // Bất hợp pháp: giữ CONTRABAND, xóa ADDICTIVE riêng (đã gộp vào CONTRABAND)
                tags.Remove("ADDICTIVE");
                if (!tags.Contains("CONTRABAND")) tags.Add("CONTRABAND");
            }
        }

        if (tags.Count == 0) tags.Add("GENERAL");
        return tags;
'@
if ($content.Contains($old5)) { $content = $content.Replace($old5, $new5); Write-Host "PATCH 5 OK" } else { Write-Host "PATCH 5 NOT FOUND" }

# =========================================================
# 6. Cập nhật version log
# =========================================================
$old6 = 'Debug.Log("[SmartInventorySort] Đã kích hoạt v1.5.3 (Filter Out Restricted Packets + Multi-Tag + Priority).");'
$new6 = 'Debug.Log("[SmartInventorySort] Đã kích hoạt v1.5.4 (Tách Tag Gây Nghiện ADDICTIVE ra khỏi CONTRABAND).");'
if ($content.Contains($old6)) { $content = $content.Replace($old6, $new6); Write-Host "PATCH 6 OK" } else { Write-Host "PATCH 6 NOT FOUND" }

# =========================================================
# 7. Comment header version
# =========================================================
$old7 = '// Tính năng nâng cấp cốt lõi v1.5.3:'
$new7 = '// Tính năng nâng cấp cốt lõi v1.5.4:'
if ($content.Contains($old7)) { $content = $content.Replace($old7, $new7); Write-Host "PATCH 7 OK" } else { Write-Host "PATCH 7 NOT FOUND" }

$old8 = '// 1. BỘ LỌC HÒM CHỨA ĐỒ THỰC THỤ (IsGeneralStorageContainer): Tự động loại bỏ'
$new8 = '// 1. BỘ LỌC HÒM CHỨA ĐỒ THỰC THỤ (IsGeneralStorageContainer): Tự động loại bỏ
// 2. TÁCH TAG ADDICTIVE: Phân định rõ ràng chất gây nghiện hợp pháp (rượu/thuốc lá/kích thích)
//    với hàng cấm thực sự (CONTRABAND/RESTRICTED/TRAFFICKING)'
if ($content.Contains($old8)) { $content = $content.Replace($old8, $new8); Write-Host "PATCH 8 OK" } else { Write-Host "PATCH 8 NOT FOUND" }

[System.IO.File]::WriteAllText($file, $content, [System.Text.Encoding]::UTF8)
Write-Host "FILE SAVED OK"
