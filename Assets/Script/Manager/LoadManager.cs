using System.Threading.Tasks;


public class LoadManager
{
    static LoadManager instance;
    public static LoadManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LoadManager();
            }
            return instance;
        }
    }

    public async Task LoadTemp()
    {
        //await AssetManager.Instance.LoadByLabelAsync("Entity");
        await AssetManager.Instance.LoadByLabelAsync("Building"); // ✅ 추가
        await AssetManager.Instance.LoadByLabelAsync("BuildingGhost"); // ✅ 추가
        await AssetManager.Instance.LoadByLabelAsync("Monster"); // ✅ 추가
        await AssetManager.Instance.LoadByLabelAsync("Bullet");
        //await AssetManager.Instance.LoadByLabelAsync("UI");
    }
}


public static class ResourceString
{
    public const string LabelUI = "UI";
    public const string LabelMonster = "Monster";
    public const string LabelBuilding = "Building";
    public const string LabelBullet = "Bullet";


    #region UI

    public const string BuildCardName = "BuildingCard";

    #endregion


    #region Bullet

    public const string TestBullet = "TestBullet";

    #endregion
}