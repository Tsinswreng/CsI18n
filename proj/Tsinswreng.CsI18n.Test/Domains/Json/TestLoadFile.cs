using System.IO;
using System.Text;
using Tsinswreng.CsCore;
using Tsinswreng.CsTreeTest;
using Tsinswreng.CsI18n.Test.Utils;

namespace Tsinswreng.CsI18n.Test.Domains.Json;

// 本文件只測「從檔案讀」這一路。
public partial class TestI18nJson {
	[Doc($$"""
	#Sum[掛上 {{nameof(I18nJson.LoadFile)}} 的用例。]
	#Descr[
	臨時檔寫在測試程式自己的輸出目錄（`AppContext.BaseDirectory`）下，
	不碰系統暫存目錄；每個用例無論成敗都會在 finally 裏刪掉。
	]
	#Prm[{{nameof(Node)}}][用例要掛的節點。]
	""")]
	public void RegisterLoadFile(ITestNode Node) {
		var Register = Node.MkTestFnRegister(
			typeof(TestI18nJson),
			[typeof(I18nJson)],
			[nameof(I18nJson.LoadFile)],
			"LoadFile:"
		);
		var R = Register.Register;
		var T = Assert.IsTrue;

		R("讀檔並攤平", async Arg => {
			var FilePath = Path.Combine(AppContext.BaseDirectory, "TestLoadFile_Lang.json");
			try {
				File.WriteAllText(FilePath, """{ "View": { "Login": "登錄" } }""", Encoding.UTF8);
				var Map = I18nJson.LoadFile(FilePath);
				T(Map.Count == 1, $"期望一條，實際 {Map.Count}");
				T(Map["View/Login"] == "登錄");
			} finally {
				// 測試資料一定要清掉，免得下次跑到殘留檔。
				if (File.Exists(FilePath)) {
					File.Delete(FilePath);
				}
			}
			return NIL;
		});

		R("檔案不存在：拋", async Arg => {
			var FilePath = Path.Combine(AppContext.BaseDirectory, "TestLoadFile_Missing.json");
			T(Chk.Throws<FileNotFoundException>(() => I18nJson.LoadFile(FilePath)));
			return NIL;
		});

		R("讀回來的東西能直接進集合", async Arg => {
			// 端到端：外部語言包 → AddLang(map) → 取文案。
			var FilePath = Path.Combine(AppContext.BaseDirectory, "TestLoadFile_En.json");
			try {
				File.WriteAllText(FilePath, """{ "Login": "Logged in" }""", Encoding.UTF8);
				var Set = new I18nSet().AddLang("en", I18nJson.LoadFile(FilePath));
				var Key = I18nKey.Mk(null, ["Login"]);
				T(Set.DfltLang == "en");
				T(Set.Cur.Get(Key) == "Logged in");
			} finally {
				if (File.Exists(FilePath)) {
					File.Delete(FilePath);
				}
			}
			return NIL;
		});
	}
}

