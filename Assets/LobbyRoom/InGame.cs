using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InGame : MonoBehaviourPunCallbacks 
{
    
    object myFrefab;
    public Vector3 spawnPos = Vector3.zero;
    public PhotonView PV;
    
    private void Awake()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        PV = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void RPC_MapSet(Vector3 _startPos)
    {
        spawnPos = _startPos;
        Debug.Log("rpc marpset");
    }

    public void MapSet(Vector3 _startPos)
    {
        PV.RPC("RPC_MapSet", RpcTarget.All, _startPos);
    }

    private void Start()
    {
        PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("characterName", out object myFrefab);
        //string map_name = PhotonNetwork.CurrentRoom.CustomProperties["map_num"].ToString();
        AudioManager.instance.PlayerOneShot(MARIO_SOUND.INGAME_BGM, true, 0);
        string map_name = WIndowManager.instance.mapName;
        BuildSystem.instance.isPlay = true;
        Debug.Log(map_name + "<< ¸ÊÀÌ¸§" + WIndowManager.instance.mapName);
        if(PhotonNetwork.IsMasterClient)
            BuildSystem.instance.MakeMap(map_name.Replace("/",""));
        //BuildSystem.instance.MakeMap("MapData_rose123_1.json");
        Debug.Log("red2" + (string)myFrefab);
        StartCoroutine(Character_Spawn((string)myFrefab));
        
    }

    IEnumerator Character_Spawn(string _a)
    {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.Instantiate(_a, spawnPos, Quaternion.identity);
    }
}
