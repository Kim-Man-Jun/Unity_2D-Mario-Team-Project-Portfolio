using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Storage;
using UnityEngine.UI;
using System.IO;
using Firebase.Extensions;
using System.Threading.Tasks;
using UnityEditor;

public class FirebaseDataManager
{

    FirebaseStorage storage;
    StorageReference storageReference;

    public Text saveQutNum;
    public Text loadQutNum;

    //버튼 액션 생성 (파일 경로랑 파일이름까지 넣어주세요)
    public void SaveStorageCustom(string file_path)
    {
        //파일 경로
        string path = Application.dataPath + "/" + file_path;
        //storage 디폴트 인스턴스 설정
        storage = FirebaseStorage.DefaultInstance;
        //storageReferene에 파이어베이스 URL 주소 설정
        storageReference = storage.GetReferenceFromUrl("gs://teamproject-supermariomaker.appspot.com");
        //byte[] bytes = File.ReadAllBytes(path);
        StorageReference uploadRef = storageReference.Child("Maps/" + file_path);
        uploadRef.PutFileAsync(path).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("실패333" + file_path + " " + task.Exception.ToString());
            }
            else
            {
                Debug.Log("성공333");
            }
        });
    }

    public void LoadStorageCustom(string file_path)
    {
        Debug.Log("load list...");
        string path = Application.dataPath + "/" + file_path;
        storage = FirebaseStorage.DefaultInstance;
        StorageReference reference = storage.GetReferenceFromUrl
            ("gs://teamproject-supermariomaker.appspot.com");
        StorageReference downloadRef = reference.Child("Maps/" + file_path);

        downloadRef.GetFileAsync(path).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                //loadQutNum.text = "성공";
                Debug.Log(path + " 파이어베이스에서 list 가져오기 성공'");
            }
            else
            {
                //loadQutNum.text = "실패";
                Debug.Log(path + " 파이어베이스에서 list 가져오기 실패'");
            }
        });
    }

    public bool LoadStorageCustom_lobby(string file_path)
    {
        //Debug.Log("load list...");
        string path = Application.dataPath + "/" + file_path;
        storage = FirebaseStorage.DefaultInstance;
        StorageReference reference = storage.GetReferenceFromUrl
            ("gs://teamproject-supermariomaker.appspot.com");
        StorageReference downloadRef = reference.Child("Maps/" + file_path);
        bool val = false;

        downloadRef.GetFileAsync(path).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                //loadQutNum.text = "성공";
                //Debug.Log(path + " 파이어베이스에서 list 가져오기 성공 lobby'");
                val = true;
            }
            else
            {
                //loadQutNum.text = "실패";
                //Debug.Log(path + " 파이어베이스에서 list 가져오기 실패 lobby'");
            }
        });

        return val;
    }

    //버튼 액션 생성
    public void SaveStorage()
    {
        //파일 경로
        string path = Application.dataPath + "/Kdm/Scenes/KdmScene.unity";
        //storage 디폴트 인스턴스 설정
        storage = FirebaseStorage.DefaultInstance;
        //storageReferene에 파이어베이스 URL 주소 설정
        storageReference = storage.GetReferenceFromUrl("gs://teamproject-supermariomaker.appspot.com");
        //byte[] bytes = File.ReadAllBytes(path);
        StorageReference uploadRef = storageReference.Child("Maps/KdmScene.unity");
        saveQutNum.text = "저장중";
        uploadRef.PutFileAsync(path).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                saveQutNum.text = "실패111";
            }
            else
            {
                Debug.Log("성공");
                saveQutNum.text = "성공111";
            }
        });

    }



    public void LoadStorage()
    {
        string path = Application.dataPath + "/KMJ/Mapdownload/KdmScene.unity";
        storage = FirebaseStorage.DefaultInstance;
        StorageReference reference = storage.GetReferenceFromUrl
            ("gs://teamproject-supermariomaker.appspot.com");
        StorageReference downloadRef = reference.Child("Maps");

        downloadRef.GetFileAsync(path).ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                loadQutNum.text = "성공222";
                Debug.Log(path);
            }
            else
            {
                loadQutNum.text = "실패222";
            }
        });
    }
}

//1. 유니티 모듈에서 IOS도 다운로드
//2. Firebase_unity_sdk 파이어베이스 공식사이트에서 다운로드
//3. 유니티에서 FirebaseStorage.unitypackage import
//4. 파이어베이스 공식 사이트에서 저장소로 쓰는 곳에 가서 프로젝트 개요 - 설정버튼
// - google-services.json 다운로드 - 유니티 Plugins에 집어넣기 
