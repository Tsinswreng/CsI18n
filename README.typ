#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Tsinswreng.CsI18n][
	鍵路徑式的國際化模型：*鍵就是 KeyNode 上的一段路徑，譯文就是一張「鍵 → 模板」表*。

	本庫只做這幾件事：
	- 鍵：`II18nKey : IKeyNode`、`I18nKey`（不再依賴 CsCfg）
	- 條目：`I18nEntry`（鍵物件 ＋ 模板；鍵直接引用應用宣告的鍵，改名即編譯錯）
	- 字典：`I18nDict`（重複鍵直接拋；格式化引擎是 `IFormatter` 介面，實現為 ICU MessageFormat）
	- 多語言集合：`I18nSet`（`AddLang`／`Use`／`Cur`／`Verify` 覆蓋率自檢）
	- 語言包 JSON：`I18nJson`（讀：嵌套攤平成「鍵 → 模板」；寫：`ToJson`／`SaveFile`，給「構建期生成每語言一個 JSON」用）

	*不依賴* CsCfg（鍵模型改用 CsKeyNode）、*不依賴* CsErr（`IErrNode` 本身就是 `IKeyNode`，
	錯誤鍵可直接查）；運行時也不讀檔——譯文由應用以 `I18nEntry` 提供，
	要從外部檔案來就自己讀、再用 `AddLang(lang, map)` 灌進去。

	#H[示例][
		```cs
		public static class KeysI18n{
			public static I18nKey Login = I18nKey.Mk(null, [nameof(Login)]);
			public static IList<II18nKey> All = [Login];
		}

		public static class MsgZhCn{
			public static IList<I18nEntry> Entries = [
				new(KeysI18n.Login, "登錄"),
			];
		}

		public static I18nSet Set { get; } = new I18nSet { DfltLang = "zh-CN" }
			.AddLang("zh-CN", MsgZhCn.Entries);

		II18n I = Set.Cur;          // 或 Set.Use("en")
		var Text = I[KeysI18n.Login];
		foreach(var Problem in Set.Verify(KeysI18n.All)) { /* 缺譯／空譯文 */ }
		```

		錯誤鍵也一樣能查（`IErrNode : IKeyNode`，鍵路徑形如 `Error/User/NotLogin`）；
		自檢可一次傳多組必需鍵，界面文案與錯誤文案分開登記：

		```cs
		foreach(var Problem in Set.Verify(KeysI18n.All, KeysErr.All)){ /* … */ }
		```

		外部語言包（譯文不由 C\# 產生時）：

		```cs
		Set.AddLang("en", I18nJson.LoadFile("Languages/en.json"));
		```
	]

	#H[語言包分檔（構建期生成）][
		把每種語言的譯文都寫進程式的話，沒用到的語言也佔發佈體積與內存。
		本庫給的是「一語言一 JSON、用到才載入」所需的零件：

		+ 寫：`I18nJson.ToJson(map)`／`I18nJson.SaveFile(path, map)`——編譯期由一個工具把
		C\# 裡的譯文導出成 `Languages/<語言>.json`
		+ 讀：`I18nJson.LoadFile(path)`
		+ 按需：`I18nSet.LangLoader`——`Use(lang)`／`Cur` 發現語言還沒加入就調它去載入；
		`HasLang(lang)` 查是否已載入
		+ 自檢：`Verify(...)` 要「所有語言都在手裡」，故這種檢查屬於構建期工具或測試，
		不是執行時（執行時只看得到當前語言）

		```cs
		var Set = new I18nSet{ DfltLang = "zh-CN" };
		Set.LangLoader = Lang => I18nJson.LoadFile(Path.Combine(Dir, Lang + ".json"));
		Set.Cur["Login"];   // 首次取用時才載入 zh-CN
		Set.Use("en");      // 這時才載入 en
		```
	]
	#H[測試][
		測試工程在 `proj/Tsinswreng.CsI18n.Test`，用 `Tsinswreng.CsTreeTest`（NuGet `0.1.1-alpha`）。
		組織方式：一個測試類對應一個被測類，同一個類的不同被測函數放不同文件
		（`Domains/<被測類>/Test<被測函數>.cs`），主檔 `_Test<被測類>.cs` 只負責組裝。

		```bash
		dotnet run --project proj/Tsinswreng.CsI18n.Test
		```

		入口傳了 `ThrowOnFailed: true`：有用例失敗時進程以非零退出碼結束，CI 只看退出碼即可。

		#H[綜合示例][
			想先看「怎麼接」就看 `proj/Tsinswreng.CsI18n.Test/Domains/ExampleUsage`：
			`ExampleApp.cs` 是一個縮小版的應用（界面文案鍵 `KeysI18n`、錯誤鍵 `KeysErr`、
			兩個語言的譯文 `MsgZhCn`／`MsgEn`、門面 `AppI18n`），
			`TestUsage.cs` 用*一條*用例按閱讀順序走七步：取文案 → 切語言 → 帶參數 → 錯誤鍵 → 漏翻兜底 →
			啟動時覆蓋率自檢 → 併入外部 JSON 語言包。它同時是回歸測試。
		]

		其餘用例覆蓋到的邊界（這些是改動時最容易無聲弄壞的地方）：
		- 字典鍵規範化固定用 `/`，不隨鍵自己的 `PathSep`
		- `Get` 無參數時不填模板、也不驚動自訂 `IFormatter`
		- 模板寫壞（`{0`、參數不足）時退回原文而不是拋，界面不會因一條爛文案崩掉
		- `I18nSet` 的默認語言是第一加入的那個，不會被後來者搶走
		- `Verify` 的守衛：鍵清單含 null、鍵的路徑為空、某組必鍵是 null、鍵組清單本身是 null
		- `I18nJson` 攤平規則與 `I18nEntry.BuildKeyStr` 一致（否則 C\# 那套鍵查不到 JSON 那套譯文）
	]
]




