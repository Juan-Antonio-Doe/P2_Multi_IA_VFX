using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

    [field: Header("Autoattach on Editor properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] protected Renderer rend { get; set; }

    [field: Header("Trap Settings")]
    [field: SerializeField] protected Material holoMat { get; set; } //El material que debe tener la trampa cuando esta en modo placeholder
    protected Material initialMat { get; set; }
    [field: SerializeField] protected int moneyCost { get; set; } = 10;
    public int MoneyCost { get { return moneyCost; } }


    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] protected bool isPlaced { get; set; } //Para comprobar que no haga su efecto antes de que isPlaced este en true
    public bool IsPlaced { get { return isPlaced; } set { isPlaced = value; } }
    [field: SerializeField, ReadOnlyField] public PlayerManager owner { get; set; }

    void Awake() {
        initialMat = rend.material;
        rend.material = holoMat;
    }

    //Se marca como colocada y actualizamos el material y su layer
    public void Place() {
        isPlaced = true;
        rend.material = initialMat;
        //Se cambia la layer a Trap para que cuente como obstaculo a la hora de intentar colocar otra trampa
        gameObject.layer = LayerMask.NameToLayer("Trap");
    }

    //Para cambiar el color del material holografico para indicar si se puede colocar o no
    public void SetHoloMatColor(Color _color) {
        float _factor = Mathf.Pow(2, 2.5f);
        Color _c = new Color(_color.r, _color.g, _color.b) * _factor;
        _c.a = holoMat.color.a;
        rend.material.color = _c;
    }

}
