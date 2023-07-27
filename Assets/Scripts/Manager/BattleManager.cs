using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Hypocrites.Manager
{
    using DB;
    using DB.Data;
    using Defines;
    using UI.TempBattleUI;
    using Skill;

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public GameObject test;

        // 필드 정보
        List<EnemyData> enemies;
        List<Member> members;

        // 스킬 사용 처리
        public GraphicRaycaster graphicRaycaster;
        List<Being> targets;

        bool isTargeting;
        Skill castingSkill;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            isTargeting = false;
        }

        void Start()
        {
            enemies = new List<EnemyData>
            {
                Database.Instance.Enemies[0],
                Database.Instance.Enemies[1]
            };

            members = new List<Member>
            {
                Database.Instance.Members[0]
            };

            targets = new List<Being>();
        }

        void Update()
        {
            if (isTargeting && Input.GetMouseButtonDown(0))
            {
                List<RaycastResult> results = new List<RaycastResult>();
                PointerEventData ped = new PointerEventData(null);

                ped.position = Input.mousePosition;

                graphicRaycaster.Raycast(ped, results);

                foreach (RaycastResult result in results)
                {
                    if (!result.gameObject.CompareTag("Target"))
                        continue;

                    Being target = null;
                    string targetName = result.gameObject.GetComponent<BattleBeingUI>().GetName();

                    if (castingSkill.TargetingType == SkillTargetingType.ONE_ENEMY)
                    {
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            if (enemies[i].Name == targetName)
                            {
                                target = enemies[i];
                                break;
                            }
                        }
                    }
                    else if (castingSkill.TargetingType == SkillTargetingType.ONE_PARTY)
                    {
                        for (int i = 0; i < enemies.Count; i++)
                        {
                            if (enemies[i].Name == targetName)
                            {
                                target = enemies[i];
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"{castingSkill.Name}({castingSkill.TargetingType})는 타겟팅 절차가 없는 스킬입니다. 확인 바랍니다.");
                    }

                    targets.Add(target);
                    isTargeting = false;

                    break;
                }
            }
        }

        #region 스킬 사용 관련
        public void UseSkill(int index)
        {
            // TODO : 타겟팅 도중엔 스킬 버튼을 비활성화, 타겟 대상에 외곽선 효과 부여
            if (isTargeting)
                return;

            if (index < 0 || index >= 4)
            {
                Debug.Log($"{index}는 존재하지 않는 스킬 슬롯입니다.");
                return;
            }

            Skill skill = members[0].SkillSlot[index];
            if (skill == null)
                return;

            targets.Clear();

            if (skill.TargetingType == SkillTargetingType.ONE_ENEMY || skill.TargetingType == SkillTargetingType.ONE_PARTY)
            {
                isTargeting = true;
                castingSkill = skill;
                WaitTargeting();
            }
            else
            {
                if (skill.TargetingType == SkillTargetingType.ALL_PARTY || skill.TargetingType == SkillTargetingType.ALL)
                    targets.AddRange(members);

                if (skill.TargetingType == SkillTargetingType.ALL_ENEMY || skill.TargetingType == SkillTargetingType.ALL)
                    targets.AddRange(enemies);

                members[0].UseSkill(skill, targets.ToArray());
            }
        }
        
        async void WaitTargeting()
        {
            await Task.Run(() =>
            {
                while (isTargeting)
                    Thread.Sleep(50);

                members[0].UseSkill(castingSkill, targets.ToArray());

                castingSkill = null;
            });
        }
        #endregion

        #region 전투 돌입 및 종료 관련
        public void BeginBattle()
        {
            GameSceneManager.Instance.LoadScene(BattleDefines.BATTLE_SCENE_NAME, true);

            Instantiate(test);
            Invoke("EndBattle", 3f);
        }

        public void EndBattle()
        {
            GameSceneManager.Instance.UnloadScene(BattleDefines.BATTLE_SCENE_NAME);
        }
        #endregion
    }
}
