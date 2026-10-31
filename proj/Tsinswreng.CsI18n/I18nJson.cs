namespace Tsinswreng.CsI18n;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tsinswreng.CsCore;

[Doc($$"""
語言包 JSON 的讀／寫。

*讀*：把（可嵌套的）JSON 攤平成「鍵 → 模板」。嵌套路徑段用 `/` 連接
（即 {{nameof(I18nDict.KeySep)}}，與 {{nameof(I18nDict.Get)}} 拼字典鍵的方式一致），
故 `{ "View": { "Common": { "Login": "登錄" } } }` 變成 `View/Common/Login → 登錄`。

*寫*：給「構建期生成語言包」用——譯文照舊寫在 C\#（鍵受編譯檢查），
構建時由一個工具把它導出成*每種語言一個 JSON*，執行時只載入真的用到的語言。
這樣沒用到的語言不會進組件體積、也不佔內存。
""")]
public static class I18nJson {
	[Doc($$"""
	讀檔並攤平。
	#Prm[{{nameof(Path)}}][UTF-8 JSON 檔路徑。]
	#Rtn[鍵 → 模板。]
	""")]
	public static IDictionary<str, str> LoadFile(str Path) {
		using var Stream = File.OpenRead(Path);
		return Load(Stream);
	}

	[Doc($$"""
	從流讀並攤平。
	#Prm[{{nameof(Stream)}}][UTF-8 JSON 流，根節點必須是物件。]
	#Rtn[鍵 → 模板。]
	#Throw[{{nameof(InvalidDataException)}}][根節點不是物件時。]
	""")]
	public static IDictionary<str, str> Load(Stream Stream) {
		var Root = JsonNode.Parse(Stream) as JsonObject
			?? throw new InvalidDataException("Language pack root must be a JSON object.");
		var R = new Dictionary<str, str>();
		Flatten(Root, [], R);
		return R;
	}

	[Doc($$"""
	從 JSON 字符串攤平。
	#Prm[{{nameof(Json)}}][JSON 文本。]
	#Rtn[鍵 → 模板。]
	""")]
	public static IDictionary<str, str> FromJson(str Json) {
		var Root = JsonNode.Parse(Json) as JsonObject
			?? throw new InvalidDataException("Language pack root must be a JSON object.");
		var R = new Dictionary<str, str>();
		Flatten(Root, [], R);
		return R;
	}

	[Doc($$"""
	把「鍵 → 模板」寫成 JSON 文本。

	*輸出是平的*：鍵原樣當屬性名（故 `Error/User/NotLogin` 就是一個屬性名），
	不拆成嵌套物件——鍵裏本來就可能含 `/`，拆了再攤回來會歧義。
	讀取端（{{nameof(Load)}}）兩種形狀都吃得下。

	*中文不轉義*：用 `UnsafeRelaxedJsonEscaping`，否則語言包裡全是 `\uXXXX`，沒法人工看一眼。
	#Prm[{{nameof(Map)}}][鍵 → 模板。]
	#Rtn[縮排過的 JSON 文本。]
	""")]
	public static str ToJson(IDictionary<str, str> Map) {
		var Obj = new JsonObject();
		foreach (var Pair in Map) {
			Obj[Pair.Key] = JsonValue.Create(Pair.Value);
		}
		return Obj.ToJsonString(new JsonSerializerOptions {
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		});
	}

	[Doc($$"""
	把「鍵 → 模板」寫成檔案（UTF-8，無 BOM）。
	#Prm[{{nameof(Path)}}][目標路徑；上層目錄不存在會自動建立。]
	#Prm[{{nameof(Map)}}][鍵 → 模板。]
	""")]
	public static void SaveFile(str Path, IDictionary<str, str> Map) {
		var Dir = PathOf(Path);
		if (Dir is { Length: > 0 } && !Directory.Exists(Dir)) {
			Directory.CreateDirectory(Dir);
		}
		File.WriteAllText(Path, ToJson(Map), new System.Text.UTF8Encoding(false));
	}

	[Doc("取路徑的目錄部分；純檔案名時回空串。")]
	static str? PathOf(str Path) {
		return System.IO.Path.GetDirectoryName(Path);
	}

	[Doc("遞歸攤平：物件往下走，字符串值按當前路徑收進字典。")]
	static void Flatten(JsonObject Obj, IList<str> Segs, IDictionary<str, str> Out) {
		foreach (var Pair in Obj) {
			var Cur = new List<str>(Segs) { Pair.Key };
			switch (Pair.Value) {
				case JsonObject Child:
					Flatten(Child, Cur, Out);
					break;
				case JsonValue Value:
					if (Value.TryGetValue<str>(out var Text)) {
						Out[str.Join(I18nDict.KeySep, Cur)] = Text;
					}
					break;
			}
		}
	}
}
