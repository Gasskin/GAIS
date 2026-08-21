using System;
using System.Collections.Generic;
using cfg.battle;

namespace Runtime
{
    partial class GAISComponent
    {
        private int[] _tags = new int[(int)EGameTag.Max];

        private static EGameTag[] _tagParentTree;

        private bool _addTagWithDirty;

        private void InitTag(List<EGameTag> initTags)
        {
            InitTagTree();
            Array.Fill(_tags, 0, 0, _tags.Length);
            if (initTags != null)
            {
                for (int i = 0; i < initTags.Count; i++)
                {
                    AddTag(initTags[i]);
                }
            }
        }

        private void ClearTag()
        {
            Array.Fill(_tags, 0, 0, _tags.Length);
        }

        private void InitTagTree()
        {
            if (_tagParentTree != null)
            {
                return;
            }
            _tagParentTree = new EGameTag[(int)EGameTag.Max];
            Array.Fill(_tagParentTree, EGameTag.None, 0, _tagParentTree.Length);

            RegisterParent(EGameTag.Freeze, EGameTag.AbnormalState);
            RegisterParent(EGameTag.Stun, EGameTag.AbnormalState);

            return;

            void RegisterParent(EGameTag child, EGameTag parent)
            {
                _tagParentTree[(int)child] = parent;
            }
        }

        public void AddTag(EGameTag tag)
        {
            if (tag is <= EGameTag.None or >= EGameTag.Max)
            {
                return;
            }
            TravelAdd(tag, 1);
        }

        public void RemoveTag(EGameTag tag)
        {
            if (tag is <= EGameTag.None or >= EGameTag.Max)
            {
                return;
            }
            TravelAdd(tag, -1);
        }
        
        public bool AddTagsWithDirty(List<EGameTag> grantedTags)
        {
            if (grantedTags == null)
            {
                return false;
            }
            _addTagWithDirty = false;
            foreach (var tag in grantedTags)
            {
                AddTag(tag);
            }
            return _addTagWithDirty;
        }
        
        public bool RemoveTagsWithDirty(List<EGameTag> grantedTags)
        {
            if (grantedTags == null)
            {
                return false;
            }
            _addTagWithDirty = false;
            foreach (var tag in grantedTags)
            {
                RemoveTag(tag);
            }
            return _addTagWithDirty;
        }

        public bool HasTag(EGameTag tag)
        {
            if (tag is <= EGameTag.None or >= EGameTag.Max)
            {
                return false;
            }
            return  _tags[(int)tag] > 0;
        }

        public bool HasAllTags(List<EGameTag> tags)
        {
            if (tags == null)
            {
                return true;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                if (!HasTag(tags[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool HasNoTags(List<EGameTag> tags)
        {
            if (tags == null)
            {
                return true;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                if (HasTag(tags[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasAnyTags(List<EGameTag> tags)
        {
            if (tags == null)
            {
                return true;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                if (HasTag(tags[i]))
                {
                    return true;
                }
            }
            return true;
        }

        private void TravelAdd(EGameTag tag, int value)
        {
            while (tag != EGameTag.None)
            {
                var oldValue = _tags[(int)tag];
                _tags[(int)tag] += value;
                _tags[(int)tag] = Math.Max(_tags[(int)tag], 0);
                var newValue = _tags[(int)tag];
                _addTagWithDirty = _addTagWithDirty || ((oldValue == 0) != (newValue == 0));
            }
        }
    }
}