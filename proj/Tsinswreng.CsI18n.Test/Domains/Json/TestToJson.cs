using System.IO;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsI18n.Test.Domains.Json;

// 本文件只測「把鍵→模板寫成 JSON」這一路（構建期生成語言包時用它）。
public partial class TestI18nJson {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nJson.ToJson)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterToJson(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nJson),
			[typeof(I18nJson)],
			[nameof(I18nJson.ToJson), nameof(I18nJson.SaveFile)],
			"ToJson:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("寫出來的是平的：鍵原樣當屬性名", async Arg => {
			// 鍵裏本來就含 `/`，拆成嵌套物件再攤回來會歧義，故輸出保持平鋪。
			var Map = new Dictionary<str, str> {
				["Login"] = "登錄",
				["Error/User/NotLogin"] = "尚未登錄",
			};
			var Json = I18nJson.ToJson(Map);
			T(Json.Contains("\"Error/User/NotLogin\""), $"應保留斜線鍵：{Json}");
			T(Json.Contains("尚未登錄"), $"中文不應被轉義：{Json}");
			return NIL;
		});

		R("寫出去能原樣讀回來", async Arg => {
			// 生成器與執行時是一對：寫的時候平、讀的時候兩種都吃得下。
			var Map = new Dictionary<str, str> {
				["Login"] = "登錄",
				["Error/User/NotLogin"] = "尚未登錄",
			};
			var Back = I18nJson.FromJson(I18nJson.ToJson(Map));
			T(Back.Count == Map.Count, $"期望 {Map.Count} 條，實際 {Back.Count}");
			T(Back["Login"] == "登錄");
			T(Back["Error/User/NotLogin"] == "尚未登錄");
			return NIL;
		});

		R("寫檔會自動建目錄", async Arg => {
			var Path = System.IO.Path.Combine(AppContext.BaseDirectory, "ToJsonTest", "en.json");
			try {
				I18nJson.SaveFile(Path, new Dictionary<str, str> { ["Login"] = "Logged in" });
				T(File.Exists(Path));
				var Back = I18nJson.LoadFile(Path);
				T(Back["Login"] == "Logged in", $"實際 {Back["Login"]}");
			} finally {
				// 測試資料要清掉。
				var Dir = System.IO.Path.GetDirectoryName(Path)!;
				if (Directory.Exists(Dir)) {
					Directory.Delete(Dir, true);
				}
			}
			return NIL;
		});
	}
}
