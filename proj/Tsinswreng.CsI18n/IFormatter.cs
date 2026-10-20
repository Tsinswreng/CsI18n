namespace Tsinswreng.CsI18n;

using System.Globalization;
using Jeffijoe.MessageFormat;
using Tsinswreng.CsCore;

[Doc($$"""
模板格式化引擎。

*為何要抽成介面*：模板不只有「把位置參數塞進去」這一種需求——
複數（`{0, plural, …}`）、性別、選擇都要語言學規則，
而且換引擎時字典與呼叫端不該跟著改。

本庫只提供一個實現：{{nameof(IcuFormatter)}}（ICU MessageFormat）。
別的語法要另外實現本介面，塞進 {{nameof(I18nDict.Formatter)}} 即可。
""")]
public interface IFormatter {
	[Doc($$"""
	把參數填進模板。
	#Prm[{{nameof(Template)}}][模板。]
	#Prm[{{nameof(Args)}}][位置參數。]
	#Rtn[填好的文字；模板有錯時應退回原文而不是拋。]
	""")]
	public str Format(str Template, obj?[] Args);
}

[Doc($$"""
格式化引擎：ICU MessageFormat（`MessageFormat` 套件）。

*模板照 ICU 語法寫*：位置參數 `{0}`、複數 `{0, plural, …}`、
數字與日期樣式都用 ICU 的寫法。`{0}` 這種最常見的寫法看起來和舊的複合格式一樣，但底層是 ICU。

*線程安全*：`MessageFormatter` 內部的解析快取是併發安全的，故可全域共用一個實例。
""")]
public class IcuFormatter : IFormatter {
	[Doc("無狀態單例。")]
	public static IcuFormatter Inst { get; } = new();

	[Doc("共用一個實例即可；模板的解析結果由套件自己快取。")]
	readonly MessageFormatter Mf = new();

	[Doc($$"""
	填參數。
	#Prm[{{nameof(Template)}}][ICU 模板，例如 `歡迎，{0}`，或複數 `{1, plural, one {# 個檔案} other {# 個}`。]
	#Prm[{{nameof(Args)}}][位置參數；會以 `"0"`、`"1"`… 為鍵交給引擎。]
	#Rtn[填好的文字；無參數或模板不合法時退回原文。]
	#Eg[
	```cs
	var F = {{nameof(IcuFormatter)}}.Inst;
	F.Format("歡迎，{0}", ["小明"]); // 歡迎，小明
	F.Format("{1, plural, one {# 個檔案} other {# 個}", ["小明", 3]); // 3 個
	```
	]
	""")]
	public str Format(str Template, obj?[] Args) {
		if (Args.Length == 0) {
			return Template;
		}
		// ICU 的參數是具名的：`{0}` 的鍵就是字符串 "0"，故先把位置參數轉成字典。
		var Data = new Dictionary<str, obj?>(Args.Length);
		for (var i = 0; i < Args.Length; i++) {
			Data[i.ToString(CultureInfo.InvariantCulture)] = Args[i];
		}
		try {
			return Mf.FormatMessage(Template, Data);
		}
		catch (Exception) {
			// 一條文案的模板寫錯（缺右括號、用了 ICU 不認的樣式名），不該讓整個界面崩掉。
			// 退回原文，畫面上看得出來是哪條文案。
			return Template;
		}
	}
}

