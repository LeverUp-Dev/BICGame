using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DS.Windows
{
    using Data.Error;
    using Data.Save;
    using Elements;
    using Enumerations;
    using Utilities;

    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;

        private MiniMap miniMap;

        private SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DSGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes;

        private int repeatedNameErrorsCount;

        public int RepeatedNameErrorsCount
        {
            get
            {
                return repeatedNameErrorsCount;
            }

            set
            {
                repeatedNameErrorsCount = value;

                if (repeatedNameErrorsCount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (repeatedNameErrorsCount == 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }


        public DSGraphView(DSEditorWindow dSEditorWindow)
        {
            editorWindow = dSEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();
            groups = new SerializableDictionary<string, DSGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();

            AddManipulators();
            AddSearchWindow();
            AddMinimap();
            AddGridBackground();

            OnElementsDeleted();
            OnGroupElementAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();
        }

        #region Overrided Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion

        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", DSDialogueType.SingleChoice));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", DSDialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("DialogueName", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }
        #endregion

        #region Element Creation
        public DSNode CreateNode(string nodeName, DSDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"DS.Elements.DS{dialogueType}Node");

            DSNode node = (DSNode) Activator.CreateInstance(nodeType);

            node.Initialize(nodeName, this, position);

            if (shouldDraw)
            {
                node.Draw();
            }

            AddUngroupedNode(node);

            return node;
        }
        

        public DSGroup CreateGroup(string title, Vector2 localMousePosition)
        {
            DSGroup group = new DSGroup(title, localMousePosition);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DSNode))
                {
                    continue;
                }

                DSNode node = (DSNode)selectedElement;

                group.AddElement(node);
            }

            return group;
        }
        #endregion

        #region Callbacks
        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DSGroup);
                Type edgeType = typeof(Edge);

                List<DSGroup> groupsToDelete = new List<DSGroup>();
                List<Edge> edgesToDelete = new List<Edge>();
                List<DSNode> nodesToDelete = new List<DSNode>();

                foreach(GraphElement element in selection)
                {
                    if (element is DSNode)
                    {
                        nodesToDelete.Add((DSNode) element);
                        continue;
                    }

                    if (element.GetType() == edgeType)
                    {
                        Edge edge = (Edge)element;

                        edgesToDelete.Add(edge);

                        continue;
                    }

                    if (element.GetType() != groupType)
                    {
                        continue;
                    }

                    DSGroup group = (DSGroup)element;

                    groupsToDelete.Add(group);
                }

                foreach (DSGroup group in groupsToDelete){
                    List<DSNode> groupNodes = new List<DSNode>();

                    foreach (GraphElement groupElement in group.containedElements)
                    {
                        if (!(groupElement is DSNode))
                        {
                            continue;
                        }

                        groupNodes.Add((DSNode)groupElement);
                    }

                    group.RemoveElements(groupNodes);

                    RemoveGroup(group);

                    RemoveElement(group);
                }

                DeleteElements(edgesToDelete);

                foreach (DSNode node in nodesToDelete)
                {
                    if (node.Group != null)
                    {
                        node.Group.RemoveElement(node);
                    }

                    RemoveUngroupedNode(node);

                    node.DisconnectAllPorts();

                    RemoveElement(node);
                }
            };
        }

        private void OnGroupElementAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup nodeGroup = (DSGroup)group;
                    DSNode node = (DSNode)element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, nodeGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSNode node = (DSNode)element;

                    RemoveGroupedNode(node, group);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup)group;

                dsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        ++RepeatedNameErrorsCount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        --RepeatedNameErrorsCount;
                    }
                }

                RemoveGroup(dsGroup);

                dsGroup.OldTitle = dsGroup.title;

                AddGroup(dsGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        DSNode nextNode = (DSNode)edge.input.node;
                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;
                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge)element;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }
        #endregion

        #region Repeated Elements
        public void AddGroup(DSGroup group)
        {
            string groupName = group.title;

            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<DSGroup> groupsList = groups[groupName].Groups;

            groupsList.Add(group);

            Color errorColor = groups[groupName].ErrorData.Color;

            group.SetErrorStyle(errorColor);

            if (groupsList.Count == 2)
            {
                ++RepeatedNameErrorsCount;

                groupsList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.OldTitle;

            List<DSGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --RepeatedNameErrorsCount;

                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0){
                groups.Remove(oldGroupName);
            }
        }

        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;
            ungroupedNodesList.Add(node);

            Color errorColor = ungroupedNodes[nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++RepeatedNameErrorsCount;

                ungroupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName.ToLower();

            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            node.ResetStyle();

            if (ungroupedNodesList.Count == 1)
            {
                --RepeatedNameErrorsCount;

                ungroupedNodesList[0].ResetStyle();
                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = groupedNodes[group][nodeName].ErrorData.Color;
            node.SetErrorStyle(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++RepeatedNameErrorsCount;

                groupedNodesList[0].SetErrorStyle(errorColor);
            }
        }

        public void RemoveGroupedNode(DSNode node, Group group)
        {
            string nodeName = node.DialogueName.ToLower();

            node.Group = null;

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            node.ResetStyle();

            if (groupedNodesList.Count == 1)
            {
                --RepeatedNameErrorsCount;

                groupedNodesList[0].ResetStyle();
                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }
        #endregion

        #region Element Addition
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddMinimap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "Dialogue System/DSGraphViewStyles.uss",
                "Dialogue System/DSNodeStyles.uss"
            );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;

        }
        #endregion

        #region Utilities
        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            repeatedNameErrorsCount = 0;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }
        #endregion
    }
}
