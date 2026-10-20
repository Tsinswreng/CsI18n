using System.Text;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Utils;

namespace Tsinswreng.CsI18n.Test.Domains.Json;

// 本文件只測「攤平 JSON 文本」這一路（FromJson 與 Load 共用同一個攤平邏輯）。
public partial class TestI18nJson {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nJson.FromJson)}} 的用例。]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterFromJson(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nJson),
			[typeof(I18nJson)],
			[nameof(I18nJson.FromJson)],
			"FromJson:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("單層物件：鍵即屬性名", async Arg => {
			var Map = I18nJson.FromJson("""
				{ "Login": "登錄", "Logout": "退出登錄" }
				""");
			T(Map.Count == 2, $"期望兩條，實際 {Map.Count}");
			T(Map["Login"] == "登錄");
			T(Map["Logout"] == "退出登錄");
			return NIL;
		});

		R("嵌套物件：路徑段用斜線連接", async Arg => {
			// 這條規則必須與 I18nDict.KeySep 一致，否則 C# 那套鍵查不到 JSON 這套譯文。
			var Map = I18nJson.FromJson("""
				{ "View": { "Common": { "Login": "登錄" } } }
				""");
			T(Map.Count == 1, $"期望一條，實際 {Map.Count}");
			T(Map.ContainsKey("View/Common/Login"), $"鍵應是 View/Common/Login，實際 {str.Join(",", Map.Keys)}");
			T(Map["View/Common/Login"] == "登錄");
			return NIL;
		});

		R("非字串值被忽略", async Arg => {
			// 語言包裏可能有版本號、說明之類的非文案值；它們不該變成字典鍵。
			var Map = I18nJson.FromJson("""
				{
					"Version": 2,
					"Enabled": true,
					"List": ["a", "b"],
					"Nothing": null,
					"Login": "登錄"
				}
				""");
			T(Map.Count == 1, $"期望只收到 Login，實際 [{str.Join(",", Map.Keys)}]");
			T(Map["Login"] == "登錄");
			return NIL;
		});

		R("空物件：空字典", async Arg => {
			var Map = I18nJson.FromJson("{}");
			T(Map.Count == 0);
			return NIL;
		});

		R("根不是物件：拋", async Arg => {
			// 陣列或裸值都沒有「鍵」可言，攤不出東西，屬於語言包寫錯。
			T(Chk.Throws<InvalidDataException>(() => I18nJson.FromJson("[1,2,3]")));
			T(Chk.Throws<InvalidDataException>(() => I18nJson.FromJson("\"just a string\"")));
			return NIL;
		});

		R("從流讀：結果與文本一致", async Arg => {
			// Load 與 FromJson 共用攤平邏輯，這條只是守住「流那條路真的通」。
			var Json = """{ "View": { "Login": "登錄" } }""";
			using var Stream = new MemoryStream(Encoding.UTF8.GetBytes(Json));
			var Map = I18nJson.Load(Stream);
			T(Map.Count == 1);
			T(Map["View/Login"] == "登錄");
			return NIL;
		});
	}
}
