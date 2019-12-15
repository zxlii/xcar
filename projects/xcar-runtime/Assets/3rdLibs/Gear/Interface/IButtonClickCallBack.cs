using UnityEngine;
namespace Gear.Runtime.UI
{
    public interface IButtonClickCallBack
    {
        void OnButtonClickHandler(GameObject value);
        void OnButtonSelectHandler(GameObject value);
    }
}