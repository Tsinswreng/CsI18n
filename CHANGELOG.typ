#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[未發佈][
	- 去除 CsCfg 依賴：`II18nKey` 改繼承 `IKeyNode`，`I18nKey` 改繼承 `KeyNode`。
	- 去除 CsErr 依賴（原本只有註釋提及）、移除未使用的 `MessageFormat` 套件引用。
	- 刪除以 `ICfgAccessor` 為源的 `I18n`；新增 `I18nDict`（字典實現）、`I18nEntry`、`I18nSet`（多語言集合 ＋ 覆蓋率自檢）。
	- `II18n.Get` 與 `OnKeyNotFound` 改收 `IKeyNode`，故錯誤鍵（`IErrNode`）可直接查文案。
	- 格式化抽出 `IFormatter`；無參數時 `Get` 直接回模板。
	- 新增 `I18nDict.FromMap`、`I18nSet.AddLang(lang, map)`、`I18nJson`（讀 JSON 語言包並以 `/` 攤平鍵路徑）。
	- `I18nSet.Verify` 改收多組必需鍵（界面文案、錯誤文案可分開登記）；鍵清單含 null 或空路徑時直接拋，
	　不再輸出「缺少文案：」這種沒有信息量的行。
	- `I18nSet.Verify` 另補兩個守衛：某組必鍵本身是 null、鍵組清單本身是 null，都直接拋
	　`ArgumentException`（原本會以 `NullReferenceException` 炸出來）。
	- 新增測試工程 `proj/Tsinswreng.CsI18n.Test`（`Tsinswreng.CsTreeTest`）：60 條用例覆蓋本庫全部公開 API，
	　含「模板寫壞退回原文」「無參數不驚動格式化引擎」「Verify 的各種守衛」這些容易被改壞的邊界。
	- 格式化引擎改為 ICU MessageFormat（`IcuFormatter`，`I18nDict.Formatter` 的值）；舊的 `string.Format` 實現移除。
	　模板一律照 ICU 語法寫（位置參數 `{0}`、需要時複數），庫裡不再養第二套語法。
	- `I18nJson` 新增寫出：`ToJson(map)`、`SaveFile(path, map)`（平鋪鍵、中文不轉義），供構建期導出語言包。
	- `I18nSet` 新增 `LangLoader`（語言不在就調它去載入）與 `HasLang`；`Use` 因而支援「用到才載入」。
	- `II18n.Get`／`OnKeyNotFound` 仍收 `IKeyNode`；理由（錯誤鍵 `IErrNode` 也是鍵、免一層轉換）與代價
	　已寫進 `OnKeyNotFound` 的 `[Doc]`。
	- 測試工程另含*綜合示例* `Domains/ExampleUsage`：一個縮小版應用（界面文案鍵／錯誤鍵／兩語言譯文／門面）
	　＋ 七步走完本庫典型用法（取文案、切語言、帶參數、錯誤鍵、漏翻兜底、覆蓋率自檢、併入外部 JSON 語言包）。
	　它既是「怎麼接」的說明，也是回歸測試。
]



