using System;
using Core.Data;
using Core.Services.Audio;
using DG.Tweening;
using TMPro;
using UI.Other;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopItemView : MonoBehaviour
    {
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _buttonIcon;
        [SerializeField] private Image _cardBackground;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private TextMeshProUGUI _costLabel;

        [Space(5), Header("Button Setup")]
        [SerializeField] private ActionButton _buyAndSelectButton;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Sprite _unselectedSprite;
        [SerializeField] private Sprite _buySprite;
        [SerializeField] private GameObject _boughtCheckmark;

        [Space(5), Header("Visual Setup")]
        [SerializeField] private Sprite _selectedCardBackgroundSprite;
        [SerializeField] private Sprite _unselectedCardBackgroundSprite;
        [SerializeField] private Image _glow;
        [SerializeField] private float _targetGlowSize = 0.955f;
        [SerializeField] private float _originalGlowSize;
        [SerializeField] private float _glowPulseAnimationDuration = 2f;

        private string _itemId = string.Empty;
        private ShopItemType _type;

        private Material _glowMaterial;

        private bool _isPurchased = false;
        private bool _isSelected = false;

        private Tween _glowPulseTween;

        public string ItemId => _itemId;
        public bool IsSelected => _isSelected;
        public ShopItemType Type => _type;

        public event Action<string> OnItemPurchased;
        public event Action<string> OnItemSelected;

        void OnDestroy()
        {
            _buyAndSelectButton.OnButtonClick -= HandleButtonClick;
            OnItemPurchased = null;
            OnItemSelected = null;

            StopPulseAnimation();
        }

        public void Init(Sprite itemSprite, string itemId, string description, int cost, ShopItemType type, bool isPurchased = false, bool isSelected = false)
        {
            _itemIcon.sprite = itemSprite;
            _itemId = itemId;
            _type = type;

            _isPurchased = isPurchased;
            _isSelected = isSelected;

            if (_glow != null)
            {
                _glowMaterial = new(_glow.material);
                _glow.material = _glowMaterial;
            }

            if(_descriptionLabel != null)
                _descriptionLabel.text = description;

            _costLabel.text = $"{cost}";
            _costLabel.gameObject.SetActive(true);

            if (isPurchased && !isSelected)
            {
                if(type == ShopItemType.Skin)
                    _buttonIcon.sprite = _unselectedSprite;
                else
                {
                    _buyAndSelectButton.gameObject.SetActive(false);
                    _boughtCheckmark.SetActive(true);
                }

                _costLabel.gameObject.SetActive(false);
            }
            else if(!isPurchased)
                _buttonIcon.sprite = _buySprite;

            if (isSelected)
                UpdateToSelectedItemView();

            _buyAndSelectButton.OnButtonClick += HandleButtonClick;
        }

        public void UpdateToPurchasedItemView(bool playSfx = false)
        {
            if (!_isPurchased)
            {
                _isPurchased = true;
                _costLabel.gameObject.SetActive(false);
                UpdateToUnselectedItemView(_type);

                if(playSfx)
                    AudioService.Instance.PlaySfx(SoundType.Item_Purchased);
            }
        }

        public void UpdateToSelectedItemView(bool playSfx = false)
        {
            if (_isPurchased)
            {
                if(_cardBackground != null)
                    _cardBackground.sprite = _selectedCardBackgroundSprite;   

                _buttonIcon.sprite = _selectedSprite;
                _costLabel.gameObject.SetActive(false);
                _isSelected = true;
                StartPulseAnimation();

                if(playSfx)
                    AudioService.Instance.PlaySfx(SoundType.Item_Selected);
            }
        }

        public void UpdateToUnselectedItemView(ShopItemType type)
        {
            if (type == ShopItemType.Skin)
            {
                if (_isSelected)
                {
                    _buttonIcon.sprite = _unselectedSprite;
                    _isSelected = false;
                }
                else if(_isPurchased && !_isSelected)
                {
                    _buttonIcon.sprite = _unselectedSprite;
                    _isSelected = false;
                }

                if(_cardBackground != null)
                    _cardBackground.sprite = _unselectedCardBackgroundSprite;   

                StopPulseAnimation();
            }
            else
            {
                _buyAndSelectButton.gameObject.SetActive(false);
                _boughtCheckmark.SetActive(true);
            }
        }

        private void StopPulseAnimation()
        {
            if (_glow == null || _glowMaterial == null)
                return;

            _glowPulseTween?.Kill();
            _glowPulseTween = null;

            _glowMaterial.SetFloat("_BoxSize", _originalGlowSize);

            var renderedMaterial = _glow.materialForRendering;
            if (renderedMaterial != null)
                renderedMaterial.SetFloat("_BoxSize", _originalGlowSize);

            _glow.gameObject.SetActive(false);
        }

        private void StartPulseAnimation()
        {
            if (_glow == null || _glowMaterial == null)
                return;

            _glow.gameObject.SetActive(true);
            _glowPulseTween?.Kill();

            _glow.SetMaterialDirty();
            var renderedMaterial = _glow.materialForRendering;
            if (renderedMaterial == null)
                return;

            renderedMaterial.SetFloat("_BoxSize", _originalGlowSize);
            _glowPulseTween = renderedMaterial
                .DOFloat(_targetGlowSize, "_BoxSize", _glowPulseAnimationDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void HandleButtonClick()
        {
            if(!_isPurchased)
                OnItemPurchased?.Invoke(_itemId);
            else if (_isPurchased && !_isSelected)
                OnItemSelected?.Invoke(_itemId);
        }
    }
}