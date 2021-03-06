﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sector : MonoBehaviour
{
    #region Private Fields

    [SerializeField] private Map map;
    [SerializeField] private Unit unit;
    [SerializeField] private Player owner;
    [SerializeField] private Sector[] adjacentSectors;
    [SerializeField] private Landmark landmark;

    #endregion

    #region Public Properties

    public Map Map
    {
        get { return map; }
        set { map = value; }
    }

    public Unit Unit
    {
        get { return unit; }
        set { unit = value; }
    }

    public Player Owner
    {
        get { return owner; }
        set
        {
            owner = value;

            // set sector color to the color of the given player
            // or gray if null
            if (owner == null)
                gameObject.GetComponent<Renderer>().material.color = Color.gray;
            else
                gameObject.GetComponent<Renderer>().material.color = owner.Color;
        }
    }

    public Sector[] AdjacentSectors
    {
        get { return adjacentSectors; }
    }

    public Landmark Landmark
    {
        get { return landmark; }
        set { landmark = value; }
    }

    #endregion

    #region Initialization

    public void Initialize()
    {

        // initialize the sector by setting its owner and unit to null
        // and determining if it contains a landmark or not


        // reset owner
        Owner = null;

        // clear unit
        unit = null;

        // get landmark (if any)
        landmark = gameObject.GetComponentInChildren<Landmark>();

    }

    #endregion

    #region Helper Methods

    public void ApplyHighlight(float amount)
    {

        // highlight a sector by increasing its RGB values by a specified amount

        Renderer currentRenderer = GetComponent<Renderer>();
        Color currentColor = currentRenderer.material.color;
        Color offset = new Vector4(amount, amount, amount, 1);
        Color newColor = currentColor + offset;

        currentRenderer.material.color = newColor;
    }

    public void RevertHighlight(float amount)
    {

        // unhighlight a sector by decreasing its RGB values by a specified amount

        Renderer currentRenderer = GetComponent<Renderer>();
        Color currentColor = currentRenderer.material.color;
        Color offset = new Vector4(amount, amount, amount, 1);
        Color newColor = currentColor - offset;

        currentRenderer.material.color = newColor;
    }

    public void ApplyHighlightAdjacent()
    {

        // highlight each sector adjacent to this one

        foreach (Sector adjacentSector in adjacentSectors)
        {
            adjacentSector.ApplyHighlight(0.2f);
        }
    }

    public void RevertHighlightAdjacent()
    {

        // unhighlight each sector adjacent to this one

        foreach (Sector adjacentSector in adjacentSectors)
        {
            adjacentSector.RevertHighlight(0.2f);
        }
    }

    public void ClearUnit()
    {

        // clear this sector of any unit

        unit = null;
    }

    void OnMouseUpAsButton()
    {

        // when this sector is clicked, determine the context
        // and act accordingly

        OnMouseUpAsButtonAccessible();

    }

    public void OnMouseUpAsButtonAccessible()
    {

        // a method of OnMouseUpAsButton that is 
        // accessible to other objects for testing


        // if this sector contains a unit and belongs to the
        // current active player, and if no unit is selected
        if (unit != null && owner.IsActive && map.game.NoUnitSelected())
        {
            // select this sector's unit
            unit.Select();
        }

        // if this sector's unit is already selected
        else if (unit != null && unit.IsSelected)
        {
            // deselect this sector's unit           
            unit.Deselect();
        }

        // if this sector is adjacent to the sector containing
        // the selected unit
        else if (AdjacentSelectedUnit() != null)
        {
            // get the selected unit
            Unit selectedUnit = AdjacentSelectedUnit();

            // deselect the selected unit
            selectedUnit.Deselect();

            // if this sector is unoccupied
            if (unit == null)
                MoveIntoUnoccupiedSector(selectedUnit);

            // if the sector is occupied by a friendly unit
            else if (unit.Owner == selectedUnit.Owner)
                MoveIntoFriendlyUnit(selectedUnit);

            // if the sector is occupied by a hostile unit
            else if (unit.Owner != selectedUnit.Owner)
                MoveIntoHostileUnit(selectedUnit, unit);
        }
    }

    public void MoveIntoUnoccupiedSector(Unit unit)
    {
        // move the selected unit into this sector
        unit.MoveTo(this);

        // advance turn state
        map.game.NextTurnState();
    }

    public void MoveIntoFriendlyUnit(Unit otherUnit)
    {
        // swap the two units
        unit.SwapPlacesWith(otherUnit);

        // advance turn state
        map.game.NextTurnState();
    }

    /// <summary>
    /// Start and resolve a conflict.
    /// </summary>
    /// <param name="attackingUnit">Attacking unit.</param>
    /// <param name="defendingUnit">Defending unit.</param>
    public void MoveIntoHostileUnit(Unit attackingUnit, Unit defendingUnit)
    {
        // if the attacking unit wins
        if (Conflict(attackingUnit, defendingUnit))
        {
            // destroy defending unit
            defendingUnit.DestroySelf();

            // move the attacking unit into this sector
            attackingUnit.MoveTo(this);
        }

        // if the defending unit wins
        else
        {
            // destroy attacking unit
            attackingUnit.DestroySelf();
        }

        // end the turn
        map.game.EndTurn();
    }

    /// <summary>
    /// Return the selected unit if it is adjacent to this sector,
    /// return null otherwise.
    /// </summary>
    /// <returns>The adjacent selected unit.</returns>
    public Unit AdjacentSelectedUnit()
    {
        // scan through each adjacent sector
        foreach (Sector adjacentSector in adjacentSectors)
        {
            // if the adjacent sector contains the selected unit,
            // return the selected unit
            if (adjacentSector.unit != null && adjacentSector.unit.IsSelected)
                return adjacentSector.unit;
        }

        // otherwise, return null
        return null;
    }

    /// <summary>
    /// Return 'true' if attacking unit wins,
    /// return 'false' if defending unit wins.
    /// </summary>
    /// <returns>The conflict.</returns>
    /// <param name="attackingUnit">Attacking unit.</param>
    /// <param name="defendingUnit">Defending unit.</param>
    bool Conflict(Unit attackingUnit, Unit defendingUnit)
    {
        /*
         * Conflict resolution is done by comparing a random roll 
         * from each unit involved. The roll is weighted based on
         * the unit's level and the amount of the associated 
         * resource the unit's owner has. Beer is associated with
         * attacking, and Knowledge is associated with defending.
         * 
         * The formula is:
         * 
         *     roll = [ a random integer with a lowerbound of 1
         *              and an upperbound of 5 + the unit's level ] 
         *           + [ the amount of the associated resource the
         *               unit's owner has ]
         * 
         * In the event of a tie, the defending unit wins the conflict
         */

        // calculate the rolls of each unit
        int attackingUnitRoll = Random.Range(1, (5 + attackingUnit.Level)) + attackingUnit.Owner.Beer;
        int defendingUnitRoll = Random.Range(1, (5 + defendingUnit.Level)) + defendingUnit.Owner.Knowledge;

        return (attackingUnitRoll > defendingUnitRoll);
    }

    #endregion
}