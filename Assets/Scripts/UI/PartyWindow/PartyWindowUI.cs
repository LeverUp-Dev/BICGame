using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hypocrites.UI.PartyWindow
{
    using DB.Data;
    using Player;

    public class PartyWindowUI : MonoBehaviour
    {
        public GameObject partyWindowPanel;
        public GameObject membersHolder;
        public GameObject memberPrefab;

        Player player;
        bool isActive;

        Dictionary<Member, GameObject> members;

        void Awake()
        {
            isActive = false;
            members = new Dictionary<Member, GameObject>();

            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
                Member member;
                PlayerData memberData = player.Members[i];

                Vector3 position = membersHolder.transform.position + i * -80 * Vector3.up;

                // �̹� ��� ������Ʈ�� ������ ���¸� ������ ����
                if ((member = FindMember(memberData.Name)) != null)
                {
                    // �߰� ����� ����� ���ɼ��� ����� ��ġ ����
                    members[member].transform.position = position;
                    member.LoadMember(memberData);
                    continue;
                }

                // ��� ������Ʈ ����
                GameObject memberObject = Instantiate(memberPrefab);
                memberObject.transform.SetParent(membersHolder.transform, false);
                memberObject.transform.position = position;

                member = memberObject.GetComponent<Member>();
                member.LoadMember(memberData);

                members[member] = memberObject;
            }
        }

        public void RemoveMemberUI(string name)
        {
            Member member = FindMember(name);
            if (member != null)
            {
                GameObject memberObject = members[member];
                members.Remove(member);
                Destroy(memberObject);

                UpdateMemberUI();
            }
        }

        Member FindMember(string name)
        {
            foreach (Member member in members.Keys)
            {
                if (member.GetMemberName() == name)
                    return member;
            }

            return null;
        }
    }
}
