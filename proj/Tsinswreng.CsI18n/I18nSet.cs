namespace Tsinswreng.CsI18n;

using Tsinswreng.CsCore;
using Tsinswreng.CsKeyNode;

[Doc($$"""
多語言集合：語言代碼 → 字典，並記住「當前語言」。

*為何在庫裡*：切語言、列語言、比對覆蓋率這三件事與應用無關，
每個用 i18n 的專案都要寫一遍；放這裡之後應用只剩「給資料」。
""")]
public class I18nSet {
	[Doc("語言代碼 → 字典。")]
	public IDictionary<str, I18nDict> Dicts { get; set; } = new Dictionary<str, I18nDict>();

	[Doc("默認語言；第一個加入的語言會自動成為默認。")]
	public str DfltLang { get; set; } = "";

	[Doc("當前語言；第一次取用時切到默認語言。")]
	public II18n Cur {
		get {
			if (_Cur is null) {
				if (string.IsNullOrWhiteSpace(DfltLang)) {
					throw new InvalidOperationException(
						$"No language has been added. Add a language, or set {nameof(DfltLang)} and {nameof(LangLoader)}."
					);
				}
				Use(DfltLang);
			}
			return _Cur!;
		}
	}
	II18n? _Cur;

	[Doc("已加入的語言代碼。")]
	public IList<str> LangCodes {
		get {
			return Dicts.Keys.ToList();
		}
	}

	[Doc($$"""
	語言還沒加入時，去哪裏拿它的「鍵 → 模板」表。

	*為何需要這條縫*：把每種語言的譯文都寫進程式，沒用到的語言也會佔組件體積與內存
	（見 `Tsinswreng.CsI18n` 的 README「語言包」章）。常見做法是每種語言一個 JSON，
	只在真的切到它時才讀。本集合因此允許一個載入器：{{nameof(Use)}} 發現語言不在就調它。

	為 null 時行為照舊：{{nameof(Use)}} 直接拋。
	""")]
	public Func<str, IDictionary<str, str>>? LangLoader { get; set; }

	[Doc($$"""
	這個語言是否已加入（即已載入）。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	""")]
	public bool HasLang(str Lang) {
		return Dicts.ContainsKey(Lang);
	}

	[Doc($$"""
	加入一個語言。
	#Prm[{{nameof(Lang)}}][語言代碼，例如 `zh-CN`、`en`。]
	#Prm[{{nameof(Map)}}][鍵 → 模板；C\# 宣告與 JSON 語言包都走這一個入口。]
	#Rtn[自身，便於鏈式加入。]
	""")]
	public I18nSet AddLang(str Lang, IDictionary<str, str> Map) {
		Dicts[Lang] = I18nDict.FromMap(Map);
		if (string.IsNullOrWhiteSpace(DfltLang)) {
			DfltLang = Lang;
		}
		return this;
	}
	[Doc($$"""
	切換當前語言。
	#Prm[{{nameof(Lang)}}][語言代碼。]
	#Rtn[該語言的字典。]
	#Throw[{{nameof(ArgumentException)}}][沒有這個語言、且沒有 {{nameof(LangLoader)}} 可以載入它時。]
	""")]
	public II18n Use(str Lang) {
		// step 1：語言不在就先問載入器要（每種語言一個語言包時，這裡才真的讀檔）。
		if (!Dicts.ContainsKey(Lang)) {
			if (LangLoader is null) {
				throw new ArgumentException(
					$"Language is not available: {Lang}. Add it, or set {nameof(LangLoader)} so it can be loaded on demand.",
					nameof(Lang)
				);
			}
			AddLang(Lang, LangLoader(Lang));
		}
		// step 2：切過去。
		_Cur = Dicts[Lang];
		return _Cur;
	}

	[Doc($$"""
	覆蓋率自檢：逐語言比對「必需鍵」與實際提供的條目。

	檢查「必需鍵缺譯」與「譯文為空」。
	不檢查「多出來的鍵」——那是允許的（例如錯誤文案不必列入必需鍵）。

	*一次報完，不在第一處就中斷*：null 鍵組、null 鍵、空路徑鍵都只是各記一條問題，
	其餘各組照樣檢查。故回傳的清單可能同時含「代碼錯」與「漏譯」兩類，
	呼叫方看完整份清單再決定怎麼處理，而不是修一條、跑一次、再撞下一條。

	#Prm[{{nameof(Groups)}}][必需鍵，可傳多組（例如界面文案鍵一組、錯誤鍵一組）。]
	#Rtn[問題列表；空表示沒問題。]
	""")]
	public IList<str> Verify(params IEnumerable<IKeyNode>[] Groups) {
		var R = new List<str>();
		var Required = new HashSet<str>();
		var GroupIdx = -1;
		foreach (var Group in Groups) {
			GroupIdx++;
			// 一組有問題不影響其他組的檢查，故記一條就往下走。
			if (Group is null) {
				// 少寫一組鍵時很容易傳成 `Verify(GroupA, null)`。
				R.Add($"Required key group {GroupIdx} is null.");
				continue;
			}
			foreach (var Key in Group) {
				if (Key is null) {
					// 靜態初始化順序出錯時，鍵清單會拿到一堆 null；放過去只會變成「缺少文案：」這種沒有信息量的行。
					R.Add($"Required key group {GroupIdx} contains a null key. This usually means a static initialisation cycle in the key class: a group class read a member of the outer class while the outer class was reading the group members.");
					continue;
				}
				var KeyStr = Key.BuildFullPath(I18nDict.KeySep);
				if (string.IsNullOrWhiteSpace(KeyStr)) {
					R.Add($"Required key group {GroupIdx} contains a key with an empty path: {Key.GetType().Name}");
					continue;
				}
				Required.Add(KeyStr);
			}
		}
		foreach (var Pair in Dicts) {
			foreach (var Key in Required) {
				if (!Pair.Value.Map.ContainsKey(Key)) {
					R.Add($"[{Pair.Key}] missing translation: {Key}");
				}
			}
			foreach (var Item in Pair.Value.Map) {
				if (string.IsNullOrWhiteSpace(Item.Value)) {
					R.Add($"[{Pair.Key}] empty translation: {Item.Key}");
				}
			}
		}
		return R;
	}
}
