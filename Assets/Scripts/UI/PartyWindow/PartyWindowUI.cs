using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.UI.PartyWindow
{
    using DB.Data;
    using UI.StatusWindow;
    using Player;

    public class PartyWindowUI : MonoBehaviour
    {
        public StatusWindowUI statusWindow;

        public GameObject partyWindowPanel;
        public GameObject membersHolder;
        public GameObject memberPrefab;

        Party player;
        bool isActive;

        Dictionary<MemberUI, GameObject> members;

        void Awake()
        {
            isActive = false;
            members = new Dictionary<MemberUI, GameObject>();

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Party>();
            player.onMemberJoined += UpdateMemberUI;
            player.onMemberLeft += RemoveMemberUI;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.U))
            {
                isActive = !isActive;
                partyWindowPanel.SetActive(isActive);

                if (isActive)
                {
                    UpdateMemberUI();
                }
            }
        }

        public void UpdateMemberUI()
        {
            for (int i = 0; i < player.Members.Count; ++i)
            {
                MemberUI member;
                Member memberData = player.Members[i];

                Vector3 position = membersHolder.transform.position + i * -80 * Vector3.up;

                // 이미 멤버 오브젝트를 생성한 상태면 정보만 갱신
                if ((member = FindMember(memberData.Name)) != null)
                {
                    // 중간 멤버가 사라질 가능성을 고려해 위치 조정
                    members[member].transform.position = position;
                    member.LoadMember(memberData);
                    continue;
                }

                // 멤버 오브젝트 생성
                GameObject memberObject = Instantiate(memberPrefab);
                memberObject.transform.SetParent(membersHolder.transform, false);
                memberObject.transform.position = position;

                member = memberObject.GetComponent<MemberUI>();
                member.onClickMemberInfo = LoadMemberStatus;
                member.LoadMember(memberData);

                members[member] = memberObject;
            }
        }

        public void RemoveMemberUI(string name)
        {
            MemberUI member = FindMember(name);
            if (member != null)
            {
                GameObject memberObject = members[member];
                members.Remove(member);
                Destroy(memberObject);

                UpdateMemberUI();
            }
        }

        MemberUI FindMember(string name)
        {
            foreach (MemberUI member in members.Keys)
            {
                if (member.GetMemberName() == name)
                    return member;
            }

            return null;
        }

        public void LoadMemberStatus(string name)
        {
            statusWindow.ToggleWindow(name);
        }
    }
}
