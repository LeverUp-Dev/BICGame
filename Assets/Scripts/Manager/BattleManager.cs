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
    using UI.BattleUI;
    using Skill;

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        public GraphicRaycaster graphicRaycaster;
        public BattleUI battleUI;

        // �ʵ� ���� ����
        List<EnemyData> enemies;
        List<Member> members;

        // ��ų Ÿ���� ����
        bool isTargeting;
        List<Being> targets;
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
                Database.Instance.Members[0],
                Database.Instance.Members[1]
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
                        for (int i = 0; i < members.Count; i++)
                        {
                            if (members[i].Name == targetName)
                            {
                                target = members[i];
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"{castingSkill.Name}({castingSkill.TargetingType})�� Ÿ���� ������ ���� ��ų�Դϴ�. Ȯ�� �ٶ��ϴ�.");
                    }

                    targets.Add(target);
                    isTargeting = false;

                    break;
                }
            }
        }

        #region ��ų ��� ����
        public void UseSkill(Member caster, int index)
        {
            // TODO : Ÿ���� ���߿� ��ų ��ư�� ��Ȱ��ȭ, Ÿ�� ��� �ܰ��� ȿ�� �ο�
            if (isTargeting)
                return;

            if (caster == null)
            {
                Debug.LogError($"MemberSkillSlotUI�� ���� ������ �������� �ʽ��ϴ�.");
                return;
            }

            if (members.Find(member => member == caster) == null)
            {
                Debug.LogError($"BattleManager�� MemberSkillSlotUI�� ������ �ִ� {caster.Name} ���ᰡ �������� �ʽ��ϴ�.");
                return;
            }

            if (index < 0 || index >= 4)
            {
                Debug.Log($"{index}�� �������� �ʴ� ��ų �����Դϴ�.");
                return;
            }

            Skill skill = caster.SkillSlot[index];
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

                caster.UseSkill(skill, targets.ToArray());
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

        #region ���� ���� �� ���� ����
        public void BeginBattle()
        {
            battleUI.Initialize(members.ToArray(), enemies.ToArray());
            GameSceneManager.Instance.LoadScene(BattleDefines.BATTLE_SCENE_NAME, true);
        }

        public void EndBattle()
        {
            GameSceneManager.Instance.UnloadScene(BattleDefines.BATTLE_SCENE_NAME);
        }
        #endregion
    }
}
