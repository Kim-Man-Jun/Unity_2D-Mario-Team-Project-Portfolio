using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static ScriptableMapInfo;

public class BuildSystem : MonoBehaviour
{
    public static BuildSystem instance;

    UI_Editor ui_Editor;
    TilemapManager tilemapManager = new TilemapManager();

    [Serializable]
    class Tiles
    {
        [SerializeField] Tile[] Tile;// { get; private set; }
        public Tile[] tile { get { return Tile; } }

        [SerializeField] string TileName;
        public string tileName { get { return TileName; } }

        [SerializeField] GameObject[] ObjectPrefab;
        public GameObject[] objectPrefab { get { return ObjectPrefab; } }
    }

    //public enum TileName
    //{
    //    ground,
    //    brick,
    //    castle,
    //    coin,
    //    flag,
    //    flower,
    //    hardBrick,
    //    iceBrick,
    //    iceCoin,
    //    MushroomRed,
    //    MushroomGreen,
    //    nomalBrick,
    //    pipe,
    //    questionBrick0,
    //    questionBrick1,
    //    star,
    //    stoneMonster,
    //    thornsBrick,
    //    goomba,
    //    turtle,
    //    boo
    //}

    [SerializeField] GameObject virtualCamera;
    [SerializeField] GameObject ThumbnailCamera;

    int backgroundNum = 0;
    float timerCount = 0;
    //int playerLifePoint;
    Vector3 playerStartPos;
    int mapScaleNum = 0;

    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Tile marioTile;

    [SerializeField] Tiles[] tiles;

    //TileName tileName;

    [SerializeField] private Grid grid;

    [SerializeField] private Tilemap TempTilemap;
    [SerializeField] private Tilemap SetTilemap;
    [SerializeField] private TilemapRenderer SetTilemapRenderer;

    int tileNum = 0;
    //[SerializeField] private Tile deleteTile;
    [SerializeField] private List<Tile> tileList;
    //Dictionary<TileName, Tile> tiles = new Dictionary<TileName, Tile>();
    Dictionary<string, int> tilesDictionary = new Dictionary<string, int>();

    public Tile[] currentTile { get; private set; } = new Tile[1];
    //[SerializeField] public TileName currentTileName { get; private set; }
    public string currentTileName { get; private set; } = null;
    GameObject[] currentTileObjectPrefab;


    List<List<object>> objectList = new List<List<object>>();
    List<List<object>> undoList = new List<List<object>>();
    List<List<object>> redoList = new List<List<object>>();

    int undoMaxCount = 10;
    int redoMaxCount = 10;

    //[Serializable]
    //public class MyTiles
    //{
    //    public Tile[] tiles;
    //}
    //[SerializeField] private MyTiles[] tiles;


    [SerializeField] bool isSetTile = false;
    int tileX = 1;
    int tileY = 1;

    //Pipe ���� ����(���� : 0, ������ : 1, �Ʒ��� : 2, ���� : 3)
    public int pipeDir { get; set; } = 0;
    Vector3Int pipeTopPosition = new Vector3Int(0, 0, -100);
    Vector3Int defaultPipeTopPosition = new Vector3Int(0, 0, -100);

    Vector3 pipeLinkPos = new Vector3(0, 0, -100);
    int dirInfo = 0;
    //List<int> brickItemListInfo = new List<int>();

    //brick


    Vector3Int pastMousePosition;


    [SerializeField] float cameraSpeed;
    [SerializeField] float cameraMoveTriggerPos;

    public bool isPlay { get; set; } = false;

    [SerializeField] Sprite[] backgroundSprite;
    [SerializeField] Sprite[] backgroundSkySprite;
    [SerializeField] SpriteRenderer[] background_ground;
    [SerializeField] SpriteRenderer[] background_sky;

    ScriptableMapInfo mapInfo;

    public RenderTexture DrawTexture;

    [SerializeField] GameObject[] mapBoundary;

    [SerializeField] Transform cameraLimitPos_start;
    [SerializeField] Transform[] cameraLimitPos_end;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


        for (int i = 0; i < tiles.Length; i++)
        {
            tilesDictionary.Add(tiles[i].tileName, i);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ui_Editor = UI_Editor.instance;

        //for (int i = 0; i < tileList.Count; i++)
        //{
        //    tiles.Add((TileName)i, tileList[i]);
        //}

        //Ÿ�� �̸����� Ÿ�ϰ� �������� ã������ ��ųʸ��� �̸��� �ε��� ����
        //Debug.Log("red3");
        //for (int i = 0; i < tiles.Length; i++)
        //{
        //    tilesDictionary.Add(tiles[i].tileName, i);
        //}

        //Debug.Log("red4");
        currentTile[0] = null;

        WIndowManager.instance.mapNum++;

    }

    // Update is called once per frame
    void Update()
    {
        if (isPlay)
        {
            return;
        }

        CameraMove();

        isSetTile = ui_Editor.IsSetTile();

        if (ui_Editor.functionEditMode == UI_Editor.FunctionEditMode.None)
            ClickSetTile();

    }

    private void CameraMove()
    {


        //����Ʈ ��ǥ
        Vector2 mousePositionInCamera = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        //��ǥ (0 ~ 1) => (-1 ~ 1) ��ȯ
        mousePositionInCamera = new Vector2(mousePositionInCamera.x * 2 - 1,
            mousePositionInCamera.y * 2 - 1);

        float moveX = 0;
        float moveY = 0;
        if (mousePositionInCamera.x >= cameraMoveTriggerPos)
            moveX = 1;
        else if (mousePositionInCamera.x <= -cameraMoveTriggerPos)
            moveX = -1;

        if (mousePositionInCamera.y >= cameraMoveTriggerPos)
            moveY = 1;
        else if (mousePositionInCamera.y <= -cameraMoveTriggerPos)
            moveY = -1;

        ////ī�޶� �̵�
        virtualCamera.transform.Translate(moveX * cameraSpeed * Time.deltaTime,
            moveY * cameraSpeed * Time.deltaTime, 0);
        //Camera.main.transform.Translate(moveX * cameraSpeed * Time.deltaTime,
        //    moveY * cameraSpeed * Time.deltaTime, 0);

        //ī�޶� ����Ʈ ����
        float posX = virtualCamera.transform.position.x;
        float posY = virtualCamera.transform.position.y;
        if (virtualCamera.transform.position.x <= cameraLimitPos_start.position.x + 11.61f)
            posX = cameraLimitPos_start.position.x + 11.61f;
        else if (virtualCamera.transform.position.x >= cameraLimitPos_end[mapScaleNum].position.x - 11.61f)
            posX = cameraLimitPos_end[mapScaleNum].position.x - 11.61f;

        if (virtualCamera.transform.position.y <= cameraLimitPos_start.position.y + 6.52f)
            posY = cameraLimitPos_start.position.y + 6.52f;
        else if (virtualCamera.transform.position.y >= cameraLimitPos_end[mapScaleNum].position.y - 6.52f)
            posY = cameraLimitPos_end[mapScaleNum].position.y - 6.52f;

        virtualCamera.transform.position = new Vector3(posX, posY, virtualCamera.transform.position.z);

        ThumbnailCamera.transform.position = virtualCamera.transform.position;
    }

    private void ClickSetTile()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //Debug.Log("���콺 ��ġ : " + mousePosition);
        //Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        Vector3Int tilemapMousePosition = SetTilemap.WorldToCell(mousePosition);

        //if (pastMousePosition != tilemapMousePosition)

        //���� Ÿ�Ͽ� �°� x,y�� ����
        if (currentTileName == "Pipe")
        {
            if (pipeDir == 0 || pipeDir == 2)
            {
                tileX = 2;
                tileY = 1;
            }
            else if (pipeDir == 1 || pipeDir == 3)
            {
                tileX = 1;
                tileY = 2;
            }

            //�ӽ� Ÿ�ϸʿ� �׷��� ���� ��ġ Ÿ�� ������
            for (int i = 0; i < tileX; i++)
            {
                for (int j = 0; j < tileY; j++)
                {
                    TempTilemap.SetTile(pastMousePosition + new Vector3Int(i, -j), null);
                }
            }
            //�ӽ� Ÿ�ϸʿ� ��ġ�� Ÿ�� ǥ��
            for (int i = 0; i < tileX; i++)
            {
                for (int j = 0; j < tileY; j++)
                {
                    TempTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), currentTile[i + j + pipeDir * 4]);
                }
            }
        }
        else if (currentTileName == "stone")
        {
            tileX = 2;
            tileY = 2;
        }
        else if (currentTileName == "Castle")
        {
            tileX = 5;
            tileY = 5;
        }
        else
        {
            tileX = 1;
            tileY = 1;
        }

        if (currentTileName == "Pipe")
        {

        }
        else
        {
            //�ӽ� Ÿ�ϸʿ� �׷��� ���� ��ġ Ÿ�� ������
            for (int i = 0; i < tileX; i++)
            {
                for (int j = 0; j < tileY; j++)
                {
                    TempTilemap.SetTile(pastMousePosition + new Vector3Int(i, -j), null);
                }
            }
            //�ӽ� Ÿ�ϸʿ� ��ġ�� Ÿ�� ǥ��
            for (int i = 0; i < tileX; i++)
            {
                for (int j = 0; j < tileY; j++)
                {
                    TempTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), currentTile[i + j * tileX]);
                }
            }
        }


        //���� ���콺 ��ġ ����
        pastMousePosition = tilemapMousePosition;

        //Ÿ�� ��ġ
        if (Input.GetMouseButton(0) && isSetTile)
        {

            if (PossibleSetTile(tilemapMousePosition))
            {
                //������Ʈ�� ������ ������������ ���
                Vector3 tilemapToWorldPoint = SetTilemap.CellToWorld(tilemapMousePosition);

                Vector3 createPos = new Vector3(tilemapToWorldPoint.x + grid.cellSize.x / 2,
                    tilemapToWorldPoint.y + grid.cellSize.x / 2);

                GameObject createObj = null;
                int dirInfo = 0;
                int prefabIndex = 0;

                if (currentTileName == "Mario") //�÷��̾� ��ġ
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        //�÷��̾� ������ġ�� �ϳ��̱� ������ �������� ������ ����
                        for (int listIndex = 0; listIndex < objectList.Count; listIndex++)
                        {
                            if ((string)objectList[listIndex][0] == "Mario")
                            {
                                SetTilemap.SetTile((Vector3Int)objectList[listIndex][5], null);
                                Destroy(((GameObject)objectList[listIndex][7]));
                                objectList.RemoveAt(listIndex);

                                break;
                            }
                        }


                        //Ÿ�ϸʿ� Ÿ�� ����
                        //SetTilemap.SetTile(tilemapMousePosition, currentTile[0]);
                        for (int i = 0; i < tileX; i++)
                        {
                            for (int j = 0; j < tileY; j++)
                            {
                                SetTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), currentTile[i + j * tileX]);
                            }
                        }

                        playerStartPos = createPos;

                        //���� ������Ʈ ����
                        //createObj = Instantiate(currentTileObjectPrefab[prefabIndex], createPos, Quaternion.Euler(0, 0, dirInfo * (-90)));
                        createObj = PhotonNetwork.Instantiate("Prefabs/Mario", createPos, Quaternion.Euler(0, 0, dirInfo * (-90)));
                        createObj.SetActive(false);


                        //����Ʈ�� ���� ���� ����(�̸�, ���� ������ġ, �׸��� ������ġ ����, �׸��� ������ġ ��, ������ ���� ������Ʈ)
                        //������Ʈ �����Ҷ��� �����
                        //objectList.Add(new List<object> { currentTileName, createPos, tilemapMousePosition,
                        //    tilemapMousePosition + new Vector3Int(tileX - 1, -(tileY - 1)), createObj });
                        objectList.Add(new List<object> { currentTileName, createPos, pipeLinkPos, dirInfo, new List<int>(),
                            tilemapMousePosition, tilemapMousePosition + new Vector3Int(tileX - 1, -(tileY - 1)), createObj });
                    }
                }
                else
                {
                    if (currentTileName == "Pipe") //������ ��ġ
                    {
                        //ù Ŭ���� ������ �Ա� ����
                        if (pipeTopPosition == defaultPipeTopPosition)
                        {
                            pipeTopPosition = tilemapMousePosition;

                            //Ÿ�ϸʿ� Ÿ�� ����
                            for (int i = 0; i < tileX; i++)
                            {
                                for (int j = 0; j < tileY; j++)
                                {
                                    SetTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), currentTile[i + j + pipeDir * 4]);
                                }
                            }

                            if (pipeDir == 2)
                                createPos += new Vector3(grid.cellSize.x, 0);
                            else if (pipeDir == 3)
                                createPos += new Vector3(0, -grid.cellSize.y);

                            prefabIndex = 0;
                        }
                        else //������ ���� ����
                        {
                            //������ ������ �Ա� �����θ� ����������� ����
                            if (pipeDir == 0 || pipeDir == 2)
                            {
                                tilemapMousePosition = new Vector3Int(pipeTopPosition.x, tilemapMousePosition.y);
                                if (pipeDir == 0 && tilemapMousePosition.y > pipeTopPosition.y)
                                    return;
                                else if (pipeDir == 2 && tilemapMousePosition.y < pipeTopPosition.y)
                                    return;
                            }
                            else if (pipeDir == 1 || pipeDir == 3)
                            {
                                tilemapMousePosition = new Vector3Int(tilemapMousePosition.x, pipeTopPosition.y);
                                if (pipeDir == 1 && tilemapMousePosition.x > pipeTopPosition.x)
                                    return;
                                else if (pipeDir == 3 && tilemapMousePosition.x < pipeTopPosition.x)
                                    return;
                            }
                            if (!PossibleSetTile(tilemapMousePosition))
                            {
                                return;
                            }

                            //Ÿ�ϸʿ� Ÿ�� ����
                            for (int i = 0; i < tileX; i++)
                            {
                                for (int j = 0; j < tileY; j++)
                                {
                                    SetTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), currentTile[i + j + 2 + pipeDir * 4]);
                                }
                            }

                            tilemapToWorldPoint = SetTilemap.CellToWorld(tilemapMousePosition);
                            createPos = new Vector3(tilemapToWorldPoint.x + grid.cellSize.x / 2,
                                tilemapToWorldPoint.y + grid.cellSize.x / 2);

                            if (pipeDir == 2)
                                createPos += new Vector3(grid.cellSize.x, 0);
                            else if (pipeDir == 3)
                                createPos += new Vector3(0, -grid.cellSize.y);

                            prefabIndex = 1;
                        }

                        dirInfo = pipeDir;

                    }
                    else //Ÿ�� ��ġ
                    {
                        //Ÿ�ϸʿ� Ÿ�� ����
                        //SetTilemap.SetTile(tilemapMousePosition, currentTile[0]);
                        for (int i = 0; i < tileX; i++)
                        {
                            for (int j = 0; j < tileY; j++)
                            {
                                SetTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), currentTile[i + j * tileX]);
                            }
                        }

                    }


                    //���� ������Ʈ ����
                    //createObj = Instantiate(currentTileObjectPrefab[prefabIndex], createPos, Quaternion.Euler(0, 0, dirInfo * (-90)));
                    if (currentTileName != "Pipe")
                        createObj = PhotonNetwork.Instantiate("Prefabs/" + currentTileName, createPos, Quaternion.Euler(0, 0, dirInfo * (-90)));
                    else
                    {
                        if (prefabIndex == 0)
                        {
                            createObj = PhotonNetwork.Instantiate("Prefabs/Pipe_top", createPos, Quaternion.Euler(0, 0, dirInfo * (-90)));
                        }
                        else
                        {
                            createObj = PhotonNetwork.Instantiate("Prefabs/Pipe_bottom", createPos, Quaternion.Euler(0, 0, dirInfo * (-90)));
                        }
                    }

                    if (currentTileName != "Pipe" && currentTileName != "Brick" &&
                        currentTileName != "Box1" && currentTileName != "Box2")
                    {
                        createObj.SetActive(false);
                    }

                    if (currentTileName == "Pipe" && createObj.GetComponent<Pipe_top>() != null)
                        createObj.GetComponent<Pipe_top>().dirInfo = dirInfo;

                    //����Ʈ�� ���� ���� ����(�̸�, ���� ������ġ, �׸��� ������ġ ����, �׸��� ������ġ ��, ������ ���� ������Ʈ)
                    //������Ʈ �����Ҷ��� �����
                    //objectList.Add(new List<object> { currentTileName, createPos, tilemapMousePosition,
                    //    tilemapMousePosition + new Vector3Int(tileX - 1, -(tileY - 1)), createObj });
                    objectList.Add(new List<object> { currentTileName, createPos, pipeLinkPos, dirInfo, new List<int>(),
                    tilemapMousePosition, tilemapMousePosition + new Vector3Int(tileX - 1, -(tileY - 1)), createObj });

                    UndoListInput(objectList[objectList.Count - 1], true);
                }

                //���� �Ǵ� ���� �� redoList Ŭ����
                RedoListClear();
            }

        }
        //Ÿ�� ����
        else if (Input.GetMouseButton(1) && isSetTile)
        {
            //SetTilemap.SetTile(tilemapMousePosition, null);
            //tilemap.DeleteCells(gridPosition, tilemapPosition);

            //�� Ÿ������ Ȯ��
            if (SetTilemap.GetTile(tilemapMousePosition) != null)
            {
                //����Ʈ���� �����ؾ��� ������Ʈ �˻�
                for (int listIndex = 0; listIndex < objectList.Count; listIndex++)
                {
                    Vector3Int startSearchPoint = ((Vector3Int)objectList[listIndex][5]);
                    Vector3Int endSearchPoint = ((Vector3Int)objectList[listIndex][6]);
                    //���콺 ��ġ�� �ش��ϴ� ������Ʈ �˻�
                    if (tilemapMousePosition.x >= startSearchPoint.x &&
                        tilemapMousePosition.x <= endSearchPoint.x &&
                        tilemapMousePosition.y <= startSearchPoint.y &&
                        tilemapMousePosition.y >= endSearchPoint.y)
                    {
                        //��ġ�� Ÿ�� ����
                        for (int i = 0; i <= endSearchPoint.x - startSearchPoint.x; i++)
                        {
                            for (int j = 0; j <= -(endSearchPoint.y - startSearchPoint.y); j++)
                            {
                                SetTilemap.SetTile(startSearchPoint + new Vector3Int(i, -j), null);
                            }
                        }
                        //((GameObject)objectList[listIndex][4]).SetActive(false);

                        UndoListInput(objectList[listIndex], false);

                        //������ ������Ʈ ����
                        Destroy((GameObject)objectList[listIndex][7]);
                        //����Ʈ ��ҿ��� ����
                        objectList.RemoveAt(listIndex);

                        //���� �Ǵ� ���� �� redoList Ŭ����
                        RedoListClear();
                    }
                }
            }

        }

        //���콺 �� ��
        if (Input.GetMouseButtonUp(0))
        {
            //������ Ÿ�� ��ġ�� ������ pipeTopPosition �ʱ�ȭ
            if (currentTileName == "Pipe")
            {
                pipeTopPosition = defaultPipeTopPosition;



                //(������ �� �κ� HardBrick���� ������(�̿ϼ�))
                ////Ÿ�ϸʿ� Ÿ�� ����
                //for (int i = 0; i < tileX; i++)
                //{
                //    for (int j = 0; j < tileY; j++)
                //    {
                //        SetTilemap.SetTile(tilemapMousePosition + new Vector3Int(i, -j), tiles[tilesDictionary["HardBrick"]].tile[0]);
                //    }
                //}
            }
        }

    }

    //���� Ÿ�� ����
    public void SetCurrentTile(string _tileName)
    {
        PastTempTileClear();

        //���� Ÿ�� ����
        if (_tileName == "Mario")
        {
            currentTile = new Tile[1] { marioTile };
            //currentTile[0] = marioTile;
            currentTileName = _tileName;
            currentTileObjectPrefab = new GameObject[1] { PlayerPrefab };
        }
        else
        {
            currentTile = tiles[tilesDictionary[_tileName]].tile;
            currentTileName = _tileName;
            currentTileObjectPrefab = tiles[tilesDictionary[_tileName]].objectPrefab;
        }


    }

    //Ÿ�� ���� �� �ӽ� Ÿ�ϸʿ� �׷��� ���� ��ġ Ÿ�� ������
    public void PastTempTileClear()
    {
        for (int i = 0; i < tileX; i++)
        {
            for (int j = 0; j < tileY; j++)
            {
                TempTilemap.SetTile(pastMousePosition + new Vector3Int(i, -j), null);
            }
        }
    }

    //Ÿ�� ��ġ�� �������� Ȯ��
    bool PossibleSetTile(Vector3Int _tilemapMousePosition)
    {
        for (int i = 0; i < tileX; i++)
        {
            for (int j = 0; j < tileY; j++)
            {
                if (SetTilemap.GetTile(_tilemapMousePosition + new Vector3Int(i, -j)) != null)
                    return false;
            }
        }

        return true;
    }

    public void UndoListInput(List<object> _objList, bool _create_true_delete_false)
    {
        undoList.Add(_objList);
        if (undoList[undoList.Count - 1].Count == 8)
            undoList[undoList.Count - 1].Add(_create_true_delete_false);
        else
            undoList[undoList.Count - 1][8] = _create_true_delete_false;

        if (undoList.Count > undoMaxCount)
        {
            undoList.RemoveAt(0);
        }


    }

    public void Undo()
    {
        if (undoList.Count > 0)
        {
            Debug.Log("bool : " + (bool)undoList[undoList.Count - 1][8]);
            Debug.Log("count : " + undoList.Count);
            //undo�� ������ ���� ���� ��
            if ((bool)undoList[undoList.Count - 1][8])
            {
                Vector3Int startSearchPoint = ((Vector3Int)undoList[undoList.Count - 1][5]);
                Vector3Int endSearchPoint = ((Vector3Int)undoList[undoList.Count - 1][6]);
                //��ġ�� Ÿ�� ����
                for (int i = 0; i <= endSearchPoint.x - startSearchPoint.x; i++)
                {
                    for (int j = 0; j <= -(endSearchPoint.y - startSearchPoint.y); j++)
                    {
                        SetTilemap.SetTile(startSearchPoint + new Vector3Int(i, -j), null);
                    }
                }

                RedoListInput(undoList[undoList.Count - 1]);
                undoList.RemoveAt(undoList.Count - 1);


                //������Ʈ ��Ȱ��ȭ
                ((GameObject)objectList[objectList.Count - 1][7]).SetActive(false);
                //����Ʈ ��ҿ��� ����
                objectList.RemoveAt(objectList.Count - 1);

            }
            else //undo�� ���� ���� �ٽ� �ǵ��� ��
            {
                Debug.Log("�ǵ�����");
                int tileX = 1;
                int tileY = 1;
                //���� Ÿ�Ͽ� �°� x,y�� ����
                if ((string)undoList[undoList.Count - 1][0] == "Pipe")
                {
                    if ((int)undoList[undoList.Count - 1][3] == 0 || (int)undoList[undoList.Count - 1][3] == 2)
                    {
                        tileX = 2;
                        tileY = 1;
                    }
                    else if ((int)undoList[undoList.Count - 1][3] == 1 || (int)undoList[undoList.Count - 1][3] == 3)
                    {
                        tileX = 1;
                        tileY = 2;
                    }
                }
                else if ((string)undoList[undoList.Count - 1][0] == "StoneMonster")
                {
                    tileX = 2;
                    tileY = 2;
                }
                else if ((string)undoList[undoList.Count - 1][0] == "castle")
                {
                    tileX = 5;
                    tileY = 5;
                }
                else
                {
                    tileX = 1;
                    tileY = 1;
                }

                //Ÿ�ϸʿ� Ÿ�� ����
                for (int i = 0; i < tileX; i++)
                {
                    for (int j = 0; j < tileY; j++)
                    {
                        SetTilemap.SetTile((Vector3Int)undoList[undoList.Count - 1][5] + new Vector3Int(i, -j),
                            tiles[tilesDictionary[(string)undoList[undoList.Count - 1][0]]].tile[i + j * tileX]);
                    }
                }

                if ((string)undoList[undoList.Count - 1][0] == "Pipe")
                {
                    //����Ʈ �Ӹ� �� ��
                    if ((Vector3)undoList[undoList.Count - 1][2] != defaultPipeTopPosition)
                    {
                        undoList[undoList.Count - 1][7] =
                            Instantiate(tiles[tilesDictionary[(string)undoList[undoList.Count - 1][0]]].objectPrefab[0],
                            (Vector3)undoList[undoList.Count - 1][1],
                            Quaternion.Euler(0, 0, (int)undoList[undoList.Count - 1][3] * (-90)));

                        ((GameObject)undoList[undoList.Count - 1][7]).GetComponent<Pipe_top>().linkObjectPos =
                            (Vector3)undoList[undoList.Count - 1][2];
                    }
                    else
                    {
                        undoList[undoList.Count - 1][7] =
                            Instantiate(tiles[tilesDictionary[(string)undoList[undoList.Count - 1][0]]].objectPrefab[1],
                            (Vector3)undoList[undoList.Count - 1][1],
                            Quaternion.Euler(0, 0, (int)undoList[undoList.Count - 1][3] * (-90)));
                    }

                    ((GameObject)undoList[undoList.Count - 1][7]).SetActive(true);
                }
                else if (currentTileName == "Brick" || currentTileName == "QuestionBrick0" || currentTileName == "IceBrick")
                {
                    undoList[undoList.Count - 1][7] =
                            Instantiate(tiles[tilesDictionary[(string)undoList[undoList.Count - 1][0]]].objectPrefab[0],
                            (Vector3)undoList[undoList.Count - 1][1],
                            Quaternion.Euler(0, 0, (int)undoList[undoList.Count - 1][3] * (-90)));

                    ((GameObject)undoList[undoList.Count - 1][7]).GetComponent<Box>().
                        Add_Item_Num((List<int>)undoList[undoList.Count - 1][4]);

                    ((GameObject)undoList[undoList.Count - 1][7]).SetActive(true);
                }
                else
                {
                    undoList[undoList.Count - 1][7] =
                            Instantiate(tiles[tilesDictionary[(string)undoList[undoList.Count - 1][0]]].objectPrefab[0],
                            (Vector3)undoList[undoList.Count - 1][1],
                            Quaternion.Euler(0, 0, (int)undoList[undoList.Count - 1][3] * (-90)));

                    ((GameObject)undoList[undoList.Count - 1][7]).SetActive(false);
                }

                objectList.Add(undoList[undoList.Count - 1]);

                RedoListInput(undoList[undoList.Count - 1]);
                undoList.RemoveAt(undoList.Count - 1);
            }
        }
    }

    public void RedoListInput(List<object> _undoList)
    {
        redoList.Add(_undoList);

        if (redoList.Count > redoMaxCount)
        {
            if ((bool)redoList[0][8])
            {
                Destroy((GameObject)redoList[0][7]);
            }
            redoList.RemoveAt(0);
        }
    }

    public void RedoListClear()
    {
        for (int i = 0; i < redoList.Count; i++)
        {
            if ((bool)redoList[i][8])
            {
                Destroy((GameObject)redoList[i][7]);
            }
        }
        redoList.Clear();
    }

    public void Redo()
    {
        if (redoList.Count > 0)
        {
            //redo�� ���� ���� ������ ��
            if ((bool)redoList[redoList.Count - 1][8])
            {
                int tileX = 1;
                int tileY = 1;
                //���� Ÿ�Ͽ� �°� x,y�� ����
                if ((string)redoList[redoList.Count - 1][0] == "Pipe")
                {
                    if ((int)redoList[redoList.Count - 1][3] == 0 || (int)redoList[redoList.Count - 1][3] == 2)
                    {
                        tileX = 2;
                        tileY = 1;
                    }
                    else if ((int)redoList[redoList.Count - 1][3] == 1 || (int)redoList[redoList.Count - 1][3] == 3)
                    {
                        tileX = 1;
                        tileY = 2;
                    }
                }
                else if ((string)redoList[redoList.Count - 1][0] == "StoneMonster")
                {
                    tileX = 2;
                    tileY = 2;
                }
                else if ((string)redoList[redoList.Count - 1][0] == "castle")
                {
                    tileX = 5;
                    tileY = 5;
                }
                else
                {
                    tileX = 1;
                    tileY = 1;
                }

                //Ÿ�ϸʿ� Ÿ�� ����
                for (int i = 0; i < tileX; i++)
                {
                    for (int j = 0; j < tileY; j++)
                    {
                        SetTilemap.SetTile((Vector3Int)redoList[redoList.Count - 1][5] + new Vector3Int(i, -j),
                            tiles[tilesDictionary[(string)redoList[redoList.Count - 1][0]]].tile[i + j * tileX]);
                    }
                }

                if ((string)redoList[redoList.Count - 1][0] == "Pipe")
                {
                    //������ �Ӹ� �� ��
                    if ((Vector3)redoList[redoList.Count - 1][2] != defaultPipeTopPosition)
                    {
                        ((GameObject)redoList[redoList.Count - 1][7]).SetActive(true);
                    }
                }
                else if (currentTileName == "Brick" || currentTileName == "QuestionBrick0" || currentTileName == "IceBrick")
                    ((GameObject)redoList[redoList.Count - 1][7]).SetActive(true);

                redoList[redoList.Count - 1].RemoveAt(redoList[redoList.Count - 1].Count - 1);
                objectList.Add(redoList[redoList.Count - 1]);

                redoList.RemoveAt(redoList.Count - 1);

                UndoListInput(objectList[objectList.Count - 1], true);
            }
            else //redo�� ������ ���� ���� ��
            {

            }
        }
    }

    //������ ���� ����
    public void PipeLinkPos_ObjectListInput(GameObject _pipeLinkObject_0, GameObject _pipeLinkObject_1)
    {
        for (int listIndex = 0; listIndex < objectList.Count; listIndex++)
        {
            if (_pipeLinkObject_0 == (GameObject)objectList[listIndex][7])
            {
                objectList[listIndex][2] = _pipeLinkObject_1.GetComponent<Pipe_top>().myTransform.position;
            }

            if (_pipeLinkObject_1 == (GameObject)objectList[listIndex][7])
            {
                objectList[listIndex][2] = _pipeLinkObject_0.GetComponent<Pipe_top>().myTransform.position;
            }
        }
    }

    //���� ������ ����, ������ ���ִ� ������ ���� ����
    public int BrickItemSet_ObjectListInput(GameObject _itemSetBrick, int _brickItemNum)
    {
        for (int listIndex = 0; listIndex < objectList.Count; listIndex++)
        {
            if (_itemSetBrick == (GameObject)objectList[listIndex][7])
            {
                ((List<int>)objectList[listIndex][4]).Add(_brickItemNum);

                return ((List<int>)objectList[listIndex][4]).Count;
            }
        }
        return 0;
    }

    //���� ������ ����, ������ ���ִ� ������ ���� ����
    //(�Ⱦ���?)
    public int BrickItemSet_ObjectListOutput(GameObject _itemSetBrick, int _brickItemNum)
    {
        for (int listIndex = 0; listIndex < objectList.Count; listIndex++)
        {
            if (_itemSetBrick == (GameObject)objectList[listIndex][7])
            {
                ((List<int>)objectList[listIndex][4]).RemoveAt(((List<int>)objectList[listIndex][4]).Count - 1);

                return ((List<int>)objectList[listIndex][4]).Count;
            }
        }
        return 0;
    }

    //������ ���ִ� ������ ���� ����
    public int BrickItemSet_ObjectListCount(GameObject _itemSetBrick)
    {
        for (int listIndex = 0; listIndex < objectList.Count; listIndex++)
        {
            if (_itemSetBrick == (GameObject)objectList[listIndex][7])
            {
                return ((List<int>)objectList[listIndex][4]).Count;
            }
        }
        return 0;
    }


    public void PlayButtonOn()
    {
        isPlay = true;
        PastTempTileClear();

        for (int i = 0; i < objectList.Count; i++)
        {
            ((GameObject)objectList[i][7]).SetActive(true);

            if (((GameObject)objectList[i][7]).GetComponent<Box>() != null)
            {
                ((GameObject)objectList[i][7]).GetComponent<Box>().Add_Item_Num((List<int>)objectList[i][4]);
                ((GameObject)objectList[i][7]).GetComponent<Box>().EnqueueAll();
            }
        }

        SetTilemapRenderer.enabled = false;

        //virtualCamera.SetActive(true);
    }

    public void StopButtonOn()
    {
        isPlay = false;

        for (int i = 0; i < objectList.Count; i++)
        {
            if ((string)objectList[i][0] == "Mario")
            {
                GameObject player;
                player = PhotonNetwork.Instantiate("Prefabs/Mario", playerStartPos, Quaternion.identity);
                player.SetActive(false);
                Destroy((GameObject)objectList[i][7]);
                objectList[i][7] = player;
            }

            if ((string)objectList[i][0] != "Pipe" && (string)objectList[i][0] != "Brick" &&
                        (string)objectList[i][0] != "Box1" && (string)objectList[i][0] != "Box2")
            {
                ((GameObject)objectList[i][7]).SetActive(false);
            }
        }

        SetTilemapRenderer.enabled = true;

        //virtualCamera.SetActive(false);
    }


    public void SaveMap()
    {
        StartCoroutine(cor_Save_Map());
    }

    IEnumerator cor_Save_Map()
    {

        FirebaseDataManager tmp = new FirebaseDataManager();
        tmp.LoadStorageCustom("list2.json");

        yield return new WaitForSeconds(4f);

        TakeScreenShot2();
        yield return new WaitForSeconds(0.5f);

        mapInfo = new ScriptableMapInfo();

        mapInfo.name = WIndowManager.instance.nickName;
        //mapInfo.levelIndex = WIndowManager.instance.mapNum;
        mapInfo.levelIndex = 0;
        mapInfo.backgroundNum = backgroundNum;
        mapInfo.timerCount = timerCount;
        mapInfo.playerLifePoint = 1;
        mapInfo.playerStartPos = playerStartPos;
        mapInfo.mapScaleNum = 0;
        for (int i = 0; i < objectList.Count; i++)
        {
            if ((string)objectList[i][0] != "Mario")
                mapInfo.createObjectInfoList.Add(new CreateObjectInfo((string)objectList[i][0], (Vector3)objectList[i][1], (Vector3)objectList[i][2], (int)objectList[i][3], (List<int>)objectList[i][4]));
        }

        tilemapManager.SaveMap(mapInfo);

        yield break;
    }

    public void LoadMap()
    {
        //tilemapManager.LoadMap();
    }

    
    

    public void MakeMap(string _mapName)
    {
        //tilemapManager.LoadMap(WIndowManager.instance.mapNum, out mapInfo);
        tilemapManager.LoadMap(_mapName, out mapInfo);

        //��� ����
        for (int i = 0; i < background_ground.Length; i++)
        {
            background_ground[i].sprite = backgroundSprite[mapInfo.backgroundNum];
        }
        for (int i = 0; i < background_sky.Length; i++)
        {
            background_sky[i].sprite = backgroundSkySprite[mapInfo.backgroundNum];
        }



        //Ÿ�̸� ����

        //�÷��̾� ������ġ ����
        if (GameObject.Find("InGame") != null)
        {
            GameObject.Find("InGame").GetComponent<InGame>().MapSet(mapInfo.playerStartPos);
            //GameObject.Find("InGame").GetComponent<InGame>().spawnPos = mapInfo.playerStartPos;
        }
        //�� ũ�� ����

        //new Vector3(0, 0, -100);
        //������Ʈ ����
        List<CreateObjectInfo> creatObjList = mapInfo.createObjectInfoList;
        for (int i = 0; i < creatObjList.Count; i++)
        {
            Debug.Log(" create.." + creatObjList[i].objectName);

            if (creatObjList[i].objectName == "Pipe")
            {
                if (creatObjList[i].pipeLinkPos != new Vector3(0, 0, -100))
                {
                    //GameObject createPipe = Instantiate(tiles[tilesDictionary["Pipe"]].objectPrefab[0], creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));
                    GameObject createPipe = PhotonNetwork.Instantiate("Prefabs/Pipe_top", creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));
                    var id = createPipe.GetComponent<PhotonView>().ViewID;
                    Debug.Log(id + " << view id ");
                    createPipe.GetComponent<PhotonView>().RPC("Sync_Pip", RpcTarget.All, id, creatObjList[i].pipeLinkPos, creatObjList[i].dirInfo, true, true);


                    
                }
                else
                {
                    PhotonNetwork.Instantiate("Prefabs/Pipe_bottom", creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));
                    //Instantiate(tiles[tilesDictionary["Pipe"]].objectPrefab[1], creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));
                }
            }
            else if (creatObjList[i].objectName == "Brick" || creatObjList[i].objectName == "Box1" || creatObjList[i].objectName == "Box2")
            {
                Debug.Log(" flag1 ");
                var a = PhotonNetwork.Instantiate("Prefabs/" + creatObjList[i].objectName, creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));
                if (a.GetComponent<Box>() != null)
                {
                    a.GetComponent<Box>().Add_Item_Num(creatObjList[i].brickListInfo);
                }
                //Instantiate(tiles[tilesDictionary[creatObjList[i].objectName]].objectPrefab[0], creatObjList[i].createPos,
                //    Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90))).GetComponent<Box>().Add_Item_Num(creatObjList[i].brickListInfo);
                //Debug.Log("������ ����Ʈ : " + creatObjList[i].brickListInfo[0] + creatObjList[i].brickListInfo[1] + creatObjList[i].brickListInfo[2]);
            }
            else if (creatObjList[i].objectName == "IceCoin" || creatObjList[i].objectName == "questionBrick1")
            {

            }
            else
            {
                Debug.Log(" flag2 ");
                var a = PhotonNetwork.Instantiate("Prefabs/" + creatObjList[i].objectName, creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));
                //Instantiate(tiles[tilesDictionary[creatObjList[i].objectName]].objectPrefab[0], creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));

                //for (int j = 0; j < tiles.Length; j++)
                //{
                //    if (creatObjList[i].objectName == tiles[j].tileName)
                //    {
                //        Instantiate(tiles[j].objectPrefab[0], creatObjList[i].createPos, Quaternion.Euler(0, 0, creatObjList[i].dirInfo * (-90)));

                //        break;
                //    }
                //}
            }
        }
    }


    public void BackgroundSet(int _backgroundNum)
    {
        backgroundNum = _backgroundNum;

        for (int i = 0; i < background_ground.Length; i++)
        {
            background_ground[i].sprite = backgroundSprite[backgroundNum];
        }
        for (int i = 0; i < background_sky.Length; i++)
        {
            background_sky[i].sprite = backgroundSkySprite[backgroundNum];
        }
    }

    public void TimerSet(float _timerCount)
    {
        timerCount = _timerCount;
    }

    public void TakeScreenShot2()
    {
        Debug.Log("shot");
        string screenShotName = DateTime.Now.ToString("yyyyMMddHHmmss");

        var width = Screen.width;
        var height = Screen.height;

        RenderTexture.active = DrawTexture;
        var texture2D = new Texture2D(DrawTexture.width, DrawTexture.height);
        texture2D.ReadPixels(new Rect(0, 0, DrawTexture.width, DrawTexture.height), 0, 0);
        texture2D.Apply();
        var data = texture2D.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + $"/MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.png", data);

    }

    IEnumerator TakeScreenShot()
    {
        Debug.Log("shot");
        yield return new WaitForEndOfFrame();
        string screenShotName = DateTime.Now.ToString("yyyyMMddHHmmss");

        //var width = Screen.width;
        //var height = Screen.height;

        RenderTexture.active = DrawTexture;
        var texture2D = new Texture2D(DrawTexture.width, DrawTexture.height);
        texture2D.ReadPixels(new Rect(0, 0, DrawTexture.width, DrawTexture.height), 0, 0);
        texture2D.Apply();
        var data = texture2D.EncodeToPNG();
        Directory.CreateDirectory(Application.dataPath + "/../ScreenShot");
        File.WriteAllBytes($"{Application.dataPath}/../ScreenShot/{screenShotName}.png", data);
        //var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        //var tex = renderTexture.EncodeToPNG


        //tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //tex.Apply();
        //Debug.Log(Application.dataPath);


        //pc

        //Application.dataPath�� �ش� ������Ʈ Assets ����.

        //�ش� ��ο� NewDirectory��� �̸��� ���� ���� ����

        //Directory.CreateDirectory(Application.dataPath + "/../ScreenShot");


        //File.WriteAllBytes($"{Application.dataPath}/../ScreenShot/{screenShotName}.png", renderTexture.);

        Debug.Log("shot end");
    }
}