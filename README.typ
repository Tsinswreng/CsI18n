#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading;

#H[Tsinswreng.CsI18n][
	Tsinswreng.CsI18n 提供一套簡單的鍵路徑式國際化模型。

	它的核心思路是：

	- 用 `II18nKey` 表示分層鍵路徑
	- 用 `II18n` / `I18n` 從配置源讀取譯文
	- 用 `{0}`、`{1}` 這類模板參數做簡單格式化

	#H[安裝][
		```bash
		dotnet add package Tsinswreng.CsI18n --version 0.0.1-alpha
		```
	]

	#H[說明][
		譯文實際存在哪裏、如何裝載，由 `ICfgAccessor` 提供。
	]
]
