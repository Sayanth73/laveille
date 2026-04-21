using System.Collections.Generic;
using System.Linq;
using LaVeillee.Core;
using LaVeillee.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaVeillee.UI
{
    /// Story 2.5 — Sélection de composition des rôles.
    /// Dialog modal : preset recommandé + mode custom (+/-).
    public class CompositionDialog : MonoBehaviour
    {
        LobbyState _lobby;
        FusionRoomManager _room;
        int _playerCount;

        readonly Dictionary<RoleId, int> _current = new();
        TextMeshProUGUI _totalText;
        TextMeshProUGUI _errorText;
        Button _validateBtn;
        readonly Dictionary<RoleId, TextMeshProUGUI> _countTexts = new();

        public static void Open(LobbyState lobby, FusionRoomManager room, int playerCount)
        {
            var go = new GameObject("[CompositionDialog]");
            var d = go.AddComponent<CompositionDialog>();
            d._lobby = lobby;
            d._room = room;
            d._playerCount = playerCount;
            d.Build();
        }

        void Build()
        {
            if (_playerCount < 5)
            {
                Debug.Log("[Composition] < 5 joueurs — composition désactivée.");
                Destroy(gameObject);
                return;
            }

            foreach (var kv in RoleCatalog.RecommendedFor(_playerCount))
                _current[kv.Key] = kv.Value;

            UIFactory.CreateScreenCanvas("CompositionCanvas", out var canvasGo);
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().sortingOrder = 500;
            var root = UIFactory.CreateFullscreen(canvasGo.transform, "Backdrop",
                new Color(0f, 0f, 0f, 0.85f));

            var panel = UIFactory.CreatePanel(root, "Panel", DesignTokens.Colors.Night700);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(960f, 1280f);

            var title = UIFactory.CreateText(panel, "Composition", UIFactory.TextStyle.H1);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -32f);
            title.rectTransform.sizeDelta = new Vector2(0f, 56f);

            var sub = UIFactory.CreateText(panel, $"Pour {_playerCount} joueurs — composition recommandée ci-dessous",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon300);
            sub.rectTransform.anchorMin = new Vector2(0f, 1f);
            sub.rectTransform.anchorMax = new Vector2(1f, 1f);
            sub.rectTransform.pivot = new Vector2(0.5f, 1f);
            sub.rectTransform.anchoredPosition = new Vector2(0f, -96f);
            sub.rectTransform.sizeDelta = new Vector2(-48f, 40f);

            // Rôles (liste)
            var roleList = new GameObject("Roles", typeof(RectTransform), typeof(VerticalLayoutGroup));
            roleList.transform.SetParent(panel, false);
            var rlrt = roleList.GetComponent<RectTransform>();
            rlrt.anchorMin = new Vector2(0f, 0.25f);
            rlrt.anchorMax = new Vector2(1f, 0.85f);
            rlrt.offsetMin = new Vector2(24f, 0f);
            rlrt.offsetMax = new Vector2(-24f, 0f);
            var vlg = roleList.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = DesignTokens.Spacing.Sm;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;

            foreach (var role in RoleCatalog.SelectableRoles)
                CreateRoleRow(rlrt, role);

            _totalText = UIFactory.CreateText(panel, "", UIFactory.TextStyle.H3);
            _totalText.rectTransform.anchorMin = new Vector2(0f, 0.17f);
            _totalText.rectTransform.anchorMax = new Vector2(1f, 0.22f);
            _totalText.rectTransform.offsetMin = new Vector2(24f, 0f);
            _totalText.rectTransform.offsetMax = new Vector2(-24f, 0f);

            _errorText = UIFactory.CreateText(panel, "", UIFactory.TextStyle.Caption,
                DesignTokens.Colors.Blood500);
            _errorText.rectTransform.anchorMin = new Vector2(0f, 0.12f);
            _errorText.rectTransform.anchorMax = new Vector2(1f, 0.17f);
            _errorText.rectTransform.offsetMin = new Vector2(24f, 0f);
            _errorText.rectTransform.offsetMax = new Vector2(-24f, 0f);

            var cancel = UIFactory.CreateButton(panel, "Annuler", UIFactory.ButtonStyle.Secondary,
                new Vector2(400f, 88f));
            var crt = cancel.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0f);
            crt.anchorMax = new Vector2(0.5f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.anchoredPosition = new Vector2(-220f, 48f);
            cancel.onClick.AddListener(Close);

            _validateBtn = UIFactory.CreateButton(panel, "Valider", UIFactory.ButtonStyle.Primary,
                new Vector2(400f, 88f));
            var vrt = _validateBtn.GetComponent<RectTransform>();
            vrt.anchorMin = new Vector2(0.5f, 0f);
            vrt.anchorMax = new Vector2(0.5f, 0f);
            vrt.pivot = new Vector2(0.5f, 0f);
            vrt.anchoredPosition = new Vector2(220f, 48f);
            _validateBtn.onClick.AddListener(Validate);

            Refresh();
        }

        void CreateRoleRow(Transform parent, RoleId role)
        {
            var row = UIFactory.CreatePanel(parent, $"Role_{role}", DesignTokens.Colors.Night900);
            row.sizeDelta = new Vector2(0f, 72f);
            var le = row.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 72f;

            var label = UIFactory.CreateText(row, $"{RoleCatalog.EmojiFor(role)}  {RoleCatalog.LabelFor(role)}",
                UIFactory.TextStyle.Body, DesignTokens.Colors.Moon100, TextAlignmentOptions.MidlineLeft);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(0.55f, 1f);
            label.rectTransform.offsetMin = new Vector2(16f, 0f);
            label.rectTransform.offsetMax = Vector2.zero;

            var minus = UIFactory.CreateButton(row, "−", UIFactory.ButtonStyle.Secondary,
                new Vector2(64f, 56f));
            var mrt = minus.GetComponent<RectTransform>();
            mrt.anchorMin = new Vector2(0.6f, 0.5f);
            mrt.anchorMax = new Vector2(0.6f, 0.5f);
            mrt.pivot = new Vector2(0.5f, 0.5f);
            mrt.anchoredPosition = Vector2.zero;
            minus.onClick.AddListener(() => Adjust(role, -1));

            var count = UIFactory.CreateText(row, "0", UIFactory.TextStyle.H3,
                DesignTokens.Colors.Moon100);
            count.rectTransform.anchorMin = new Vector2(0.73f, 0f);
            count.rectTransform.anchorMax = new Vector2(0.83f, 1f);
            count.rectTransform.offsetMin = Vector2.zero;
            count.rectTransform.offsetMax = Vector2.zero;
            _countTexts[role] = count;

            var plus = UIFactory.CreateButton(row, "+", UIFactory.ButtonStyle.Secondary,
                new Vector2(64f, 56f));
            var prt = plus.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0.93f, 0.5f);
            prt.anchorMax = new Vector2(0.93f, 0.5f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = Vector2.zero;
            plus.onClick.AddListener(() => Adjust(role, +1));
        }

        void Adjust(RoleId role, int delta)
        {
            _current.TryGetValue(role, out var cur);
            var next = Mathf.Max(0, cur + delta);
            _current[role] = next;
            Refresh();
        }

        void Refresh()
        {
            foreach (var kv in _countTexts)
            {
                _current.TryGetValue(kv.Key, out var c);
                kv.Value.text = c.ToString();
            }
            int total = _current.Values.Sum();
            _totalText.text = $"Total : {total} / {_playerCount}";
            _totalText.color = total == _playerCount
                ? DesignTokens.Colors.Fire500
                : DesignTokens.Colors.Moon300;

            var ok = RoleCatalog.IsValid(_current, _playerCount, out var err);
            _errorText.text = ok ? "" : err;
            _validateBtn.interactable = ok;
        }

        void Validate()
        {
            var roles = new List<int>();
            var counts = new List<int>();
            foreach (var kv in _current)
            {
                if (kv.Value <= 0) continue;
                roles.Add((int)kv.Key);
                counts.Add(kv.Value);
            }
            _lobby.SetCompositionLocal(roles.ToArray(), counts.ToArray());
            Close();
        }

        void Close() => Destroy(gameObject);
    }
}
