using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public bool isPlaced = false; //Para comprobar que no haga su efecto antes de que isPlaced este en true
    public Material holoMat; //El material que debe tener la trampa cuando esta en modo placeholder
    public Renderer rend;

    private Material initialMat;

    private void Awake()
    {
        initialMat = rend.material;
        rend.material = holoMat;
    }

    public void Place() //Se marca como colocada y actualizamos el material y su layer
    {
        isPlaced = true;
        rend.material = initialMat;
        //Se cambia la layer a Trap para que cuente como obstaculo a la hora de intentar colocar otra trampa
        gameObject.layer = LayerMask.NameToLayer("Trap");
    }

    public void SetHoloMatColor(Color _color) //Para cambiar el color del material holografico para indicar si se puede colocar o no
    {
        float _factor = Mathf.Pow(2, 2.5f);
        Color _c = new Color(_color.r, _color.g, _color.b) * _factor;
        _c.a = holoMat.color.a;
        rend.material.color = _c;
    }

}
