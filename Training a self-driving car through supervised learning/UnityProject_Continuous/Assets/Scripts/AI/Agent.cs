﻿

#region Includes
using System;
using System.Collections.Generic;
#endregion

/// <summary>
/// Class that combines a genotype and a feedforward neural network (FNN).
/// </summary>
public class Agent : IComparable<Agent>
{
    #region Members
    /// <summary>
    /// The underlying genotype of this agent.
    /// </summary>
    public Genotype Genotype
    {
        get;
        private set;
    }

    /// <summary>
    /// The feedforward neural network which was constructed from the genotype of this agent.
    /// </summary>

    private bool isAlive = false;
    /// <summary>
    /// Whether this agent is currently alive (actively participating in the simulation).
    /// </summary>
    public bool IsAlive
    {
        get { return isAlive; }
        private set
        {
            if (isAlive != value)
            {
                isAlive = value;

                if (!isAlive && AgentDied != null)
                    AgentDied(this);
            }
        }
    }
    /// <summary>
    /// Event for when the agent died (stopped participating in the simulation).
    /// </summary>
    public event Action<Agent> AgentDied;
    #endregion

    #region Constructors
    /// <summary>
    /// Initialises a new agent from given genotype, constructing a new feedfoward neural network from
    /// the parameters of the genotype.
    /// </summary>
    /// <param name="genotype">The genotpye to initialise this agent from.</param>
    /// <param name="topology">The topology of the feedforward neural network to be constructed from given genotype.</param>
    public Agent(Genotype genotype, NeuralLayer.ActivationFunction defaultActivation, params uint[] topology)
    {
        IsAlive = false;
        this.Genotype = genotype;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Resets this agent to be alive again.
    /// </summary>
    public void Reset()
    {
        Genotype.Evaluation = 0;
        Genotype.Fitness = 0;
        IsAlive = true;
    }

    /// <summary>
    /// Kills this agent (sets IsAlive to false).
    /// </summary>
    public void Kill()
    {
        IsAlive = false;
    }

    #region IComparable
    /// <summary>
    /// Compares this agent to another agent, by comparing their underlying genotypes.
    /// </summary>
    /// <param name="other">The agent to compare this agent to.</param>
    /// <returns>The result of comparing the underlying genotypes of this agent and the given agent.</returns>
    public int CompareTo(Agent other)
    {
        return this.Genotype.CompareTo(other.Genotype);
    }
    #endregion
    #endregion
}

