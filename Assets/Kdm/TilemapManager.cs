using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
//using static ScriptableMapInfo;
//using static UnityEditor.PlayerSettings;

public class TilemapManager// : MonoBehaviour
{

    public List<string> map_list;
    ////inspector 노출
    [SerializeField] private Tilemap _gridMap, _tempMap, _setTilemap;
    [SerializeField] public int _levelIndex = 0;

    public int _backgroundNum;
    public float _timerCount;
    public int _playerLifePoint;
    public Vector3 _playerStartPos;
    public int _mapScaleNum;

    public static void SaveLevelFile(ScriptableLevel level)
    {

        ////newlevel을 asset으로 저장하기, 파일명은 수정가능, 리소스는 필수임
        //AssetDatabase.CreateAsset(level, $"Assets/Resources/Levels/{level.name}.asset");
        ////asset 저장 확정
        //AssetDatabase.SaveAssets();
        ////Unity Project를 최신 상태로 갱신 즉 프로젝트 전체의 파일 구성을 체크
        //AssetDatabase.Refresh();
    }

    public void JsonSave_list(List<string> _list)
    {
        FileListJson saveData = new FileListJson();
        saveData.list = _list;


        string path = Path.Combine(Application.dataPath, "list2.json");


        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(path, json);
    }

    public void JsonLoad_list()
    {
        FileListJson saveData = new FileListJson();

        string path = Path.Combine(Application.dataPath, "list2.json");
        if (!File.Exists(path))
        {
            //SaveData();
        }
        else
        {
            string loadJson = File.ReadAllText(path);
            saveData = JsonUtility.FromJson<FileListJson>(loadJson);
            //saveData.
            if (saveData != null)
            {
                map_list = saveData.list;
                for (int i = 0; i < saveData.list.Count; i++)
                {
                    //Debug.Log(" mp data ...  : " + saveData.list[i]);


                }
            }
        }


    }


    //맵 저장하는 메서드
    public void SaveMap(ScriptableMapInfo _mapInfo)
    {
        //ScriptableMapInfo saveData = new ScriptableMapInfo();
        //파일로 있는 list.json 읽어와.
        JsonLoad_list();
        bool isExist = false; ;

        string path = Path.Combine(Application.dataPath, $"MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.json");
        if (map_list != null)
        {
            Debug.Log("flag1  " + map_list.Count + " ");
            for (int i = 0; i < map_list.Count; i++)
            {
                if (map_list[i] == $"/MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.json") isExist = true;
            }

            if (!isExist)
            {
                Debug.Log("flag2");
                map_list.Add($"/MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.png");
                map_list.Add($"/MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.json");
            }
        }
        Debug.Log("flag3");
        //파일로 list.json 다시 저장.
        JsonSave_list(map_list);


        var newObj = new ScriptableMapInfo();

        //newObj.name = $"Level Obj {_mapInfo.levelIndex}";

        //값 긁어오는거 필요
        newObj.name = _mapInfo.name;
        newObj.levelIndex = _mapInfo.levelIndex;
        newObj.backgroundNum = _mapInfo.backgroundNum;
        newObj.timerCount = _mapInfo.timerCount;
        newObj.playerLifePoint = _mapInfo.playerLifePoint;
        newObj.playerStartPos = _mapInfo.playerStartPos;
        newObj.mapScaleNum = _mapInfo.mapScaleNum;


        newObj.createObjectInfoList = _mapInfo.createObjectInfoList;

        string json = JsonUtility.ToJson(newObj, true);

        //맵 데이터 제이슨 파일로 저장
        File.WriteAllText(path, json);
        Debug.Log(path + " 로 맵 데이터 저장" + WIndowManager.instance.nickName + " 실제 닉네임");

        FirebaseDataManager tmp = new FirebaseDataManager();

        //파이어베이스에 리스트 제이슨 올림
        tmp.SaveStorageCustom("/list2.json");
        //파이어베이스에 맵 데이터 올림
        tmp.SaveStorageCustom($"/MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.png");
        tmp.SaveStorageCustom($"/MapData_{WIndowManager.instance.nickName}_{WIndowManager.instance.mapNum}.json");

    }

    //public void ClearMap()
    //{
    //    //지역변수 var maps는 Tilemap 오브젝트
    //    var maps = FindObjectsOfType<Tilemap>();
    //    var obj = GameObject.FindGameObjectsWithTag("Object");

    //    //maps에 대해 밑에 적힌 작업을 시작함
    //    foreach (var tilemap in maps)
    //    {
    //        //타일맵 제거
    //        tilemap.ClearAllTiles();
    //    }

    //    foreach (var Obj in obj)
    //    {
    //        DestroyImmediate(Obj);
    //    }
    //}

    public void LoadMap(string mapName, out ScriptableMapInfo _scriptableMapInfo)
    {
        //Resources 폴더에서 스크립터블 오브젝트인 ScriptableLevel에
        //Level *.asset 파일을 level에 덮어씌움
        //var level = Resources.Load<ScriptableLevel>($"Levels/Level {_levelIndex}");
        //var obj = Resources.Load<ScriptableMapInfo>($"Levels/Level Obj {_levelIndex}");

        //_scriptableMapInfo = new ScriptableMapInfo();

        //ScriptableMapInfo saveData = new ScriptableMapInfo();

        string path = Path.Combine(Application.dataPath, mapName);
        Debug.Log(path + " 가 접근이 안대" + " 맵이름은" + mapName);
        if (!File.Exists(path))
        {
            //SaveData();
        }

        string loadJson = File.ReadAllText(path);
        _scriptableMapInfo = JsonUtility.FromJson<ScriptableMapInfo>(loadJson);
        //saveData.
        //if (_scriptableMapInfo != null)
        //{
        //    for (int i = 0; i < _scriptableMapInfo.createObjectInfoList.Count; i++)
        //    {
        //        Debug.Log(_scriptableMapInfo.createObjectInfoList[i].objectName);


        //    }
        //}

        //_scriptableMapInfo = obj;

        ////만약 파일이 없었을 경우
        //if (obj == null)// || level == null)
        //{
        //    //디버그 로그를 띄운 뒤 함수 종료
        //    Debug.LogError($"Level {_levelIndex} does now exist.");
        //    return;
        //}



        ////불러오기 전 맵 정리
        ////ClearMap();

        //CreateObjectInfo createObjectInfo = new CreateObjectInfo();

        //Vector3 playerStartPos = obj.playerStartPos;

        //Debug.Log(playerStartPos);

        //foreach (var savedObj in obj.createObjectInfoList)
        //{
        //    switch (savedObj)
        //    {

        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }

        //}



        ////땅 타일, 한개씩 반복작업 용 foreach
        //foreach (var savedTile in level.GroundTiles)
        //{
        //    //switch에 따라 처리 바꾸기, level에 있는 enum 참고
        //    switch (savedTile.Tile.Type)
        //    {
        //        case TileType.Field:
        //        case TileType.Brick:
        //        case TileType.Ice:
        //        case TileType.Grid:
        //        case TileType.Castle:
        //            //_groundMap에 타일들 처리해주기
        //            SetTile(_gridMap, savedTile);
        //            break;
        //        default:
        //            //만약 처리 중 값이 상정범위 내를 넘어갔을 경우 에러 발생
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}

        ////유닛타일, 위 땅타일하고 똑같음
        //foreach (var savedTile in level.UnitTiles)
        //{
        //    switch (savedTile.Tile.Type)
        //    {
        //        case TileType.Field:
        //        case TileType.Brick:
        //        case TileType.Ice:
        //        case TileType.Grid:
        //        case TileType.Castle:
        //            SetTile(_tempMap, savedTile);
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}

        //void SetTile(Tilemap map, SavedTile tile)
        //{
        //    //타일을 포지션 및 타일 번호에 따라서 쭉 깔아주는 메서드
        //    map.SetTile(tile.Position, tile.Tile);
        //}

        //}

    }




    //level 구조체 시작
    public struct Level
    {
        public int LevelIndex;
        public List<SavedTile> GroundTiles;
        public List<SavedTile> UnitTiles;

        //데이터를 직렬화하여 문자열로 변환
        public string Serialize()
        {
            //string을 써도 됐지만 stringBuilder를 쓴 이유는 문자열 변경이 잦기 때문에 사용함
            var builder = new StringBuilder();
            //문자열 시작에 g[ cnrkgka
            builder.Append("g[");

            //foreach 반복문
            foreach (var groundTile in GroundTiles)
            {
                //땅타일의 type을 숫자로 변환, position은 문자열로 변환 후 stringBuilder에 추가함
                builder.Append($"{(int)groundTile.Tile.Type}({groundTile.Position.x}," +
                    $" {groundTile.Position.y})");
            }
            //데이터의 끝
            builder.Append("]");

            //추가했던 정보들을 문자열로 변환
            return builder.ToString();
        }
    }

}

