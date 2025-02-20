using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySoundData
{
    // [KR] 적 타입 식별자 (예: crab, soldier, boss)
    public string enemyType;
    // [KR] 사망 효과음 배열
    public string[] deathClips;
    // [KR] 공격 효과음 배열 (근접, 원거리 포함)
    public string[] attackClips;
}

[Serializable]
public class EnemySoundList
{
    // [KR] JSON 파일 내에 존재하는 모든 적 사운드 데이터를 배열로 저장
    public EnemySoundData[] enemySounds;
}

/// <summary>
/// Resources 폴더 내의 JSON 파일을 로드하여 적 사운드 데이터를 파싱하는 클래스입니다.
/// </summary>
public class EnemySoundParser : MonoBehaviour
{
    // [KR] 적 타입을 key로, 해당 사운드 데이터를 value로 저장하는 딕셔너리
    public Dictionary<string, EnemySoundData> enemySoundMap = new Dictionary<string, EnemySoundData>();

    /// <summary>
    /// Resources 폴더에서 "EnemySounds.json" 파일을 로드하고, 데이터를 파싱하여 딕셔너리에 저장합니다.
    /// </summary>
    public void LoadEnemySounds()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("EnemySounds");
        if (jsonText == null)
        {
            Debug.LogError("EnemySounds.json 파일을 찾을 수 없습니다.");
            return;
        }

        EnemySoundList soundList = JsonUtility.FromJson<EnemySoundList>(jsonText.text);
        if (soundList == null || soundList.enemySounds == null)
        {
            Debug.LogError("JSON 파싱에 실패했습니다.");
            return;
        }

        foreach (EnemySoundData data in soundList.enemySounds)
        {
            enemySoundMap[data.enemyType] = data;
            Debug.Log("적 사운드 데이터 로드 완료: " + data.enemyType);
        }
    }

    /// <summary>
    /// 지정된 적 타입의 사망 효과음 배열을 반환합니다.
    /// </summary>
    /// <param name="enemyType">적 타입 문자열</param>
    /// <returns>사망 효과음 문자열 배열</returns>
    public string[] GetDeathClips(string enemyType)
    {
        if (enemySoundMap.TryGetValue(enemyType, out EnemySoundData data))
        {
            return data.deathClips;
        }
        Debug.LogWarning("해당 적 타입의 사망 효과음 데이터가 없습니다: " + enemyType);
        return null;
    }

    /// <summary>
    /// 지정된 적 타입의 공격 효과음 배열을 반환합니다.
    /// </summary>
    /// <param name="enemyType">적 타입 문자열</param>
    /// <returns>공격 효과음 문자열 배열</returns>
    public string[] GetAttackClips(string enemyType)
    {
        if (enemySoundMap.TryGetValue(enemyType, out EnemySoundData data))
        {
            return data.attackClips;
        }
        Debug.LogWarning("해당 적 타입의 공격 효과음 데이터가 없습니다: " + enemyType);
        return null;
    }
}
