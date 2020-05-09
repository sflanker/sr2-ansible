using System;
using System.Collections.Generic;
using System.Reflection;
using ModApi.Craft;
using ModApi.Craft.Parts;
using ModApi.Craft.Program;
using ModApi.Craft.Program.Expressions;
using ModApi.Craft.Program.Instructions;
using ModApi.GameLoop;
using ModApi.GameLoop.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Craft.Parts.Modifiers {
    public class PhiloticParallaxScript : PartModifierScript<PhiloticParallaxData>, IFlightPostStart {
        private static event EventHandler<PhiloticParallaxEventArgs> PhiloticParallaxEvent;

        private IFuelSource _battery;
        private FlightProgramScript _flightProgramScript;

        private EventHandler<PhiloticParallaxEventArgs> _philoticParallaxEventListener;

        public override void OnCraftLoaded(ICraftScript craftScript, bool movedToNewCraft) {
            this.OnCraftStructureChanged(craftScript);
        }

        public override void OnCraftStructureChanged(ICraftScript craftScript) {
            this._battery = this.PartScript.BatteryFuelSource;
        }

        public void FlightPostStart(in FlightFrameData frame) {
            Debug.Log("FlightPostStart");
            if (Game.InFlightScene) {
                Debug.Log("InFlightScene");
                Debug.Assert(this.PartScript != null, "ASSERT FAILED: this.PartScript != null");
                this._flightProgramScript = this.PartScript.GetModifier<FlightProgramScript>();
                if (this._flightProgramScript != null) {
                    // For every broadcast in this Flight Program inject a special receiver that will trigger a Philotic Parallax message
                    var messages = new HashSet<String>();
                    Debug.Assert(this._flightProgramScript.FlightProgram != null, "ASSERT FAILED: this._flightProgramScript.FlightProgram != null");
                    Debug.Assert(this._flightProgramScript.FlightProgram.RootInstructions != null, "ASSERT FAILED: this._flightProgramScript.FlightProgram.RootInstructions != null");
                    var instructions = new Queue<ProgramInstruction>(this._flightProgramScript.FlightProgram.RootInstructions);
                    while (instructions.Count > 0) {
                        var instruction = instructions.Dequeue();
                        Debug.Assert(instruction != null, "ASSERT FAILED: instruction != null");
                        if (instruction is BroadcastMessageInstruction broadcast) {
                            var messageExpression = broadcast.GetExpression(0);
                            if (messageExpression is ConstantExpression constant && constant.ExpressionResult != null && constant.ExpressionResult.TextValue.StartsWith("tx_")) {
                                messages.Add(constant.ExpressionResult.TextValue);
                            } else {
                                Debug.LogWarning(
                                    $"Encountered a Broadcast instruction with an unsupported message expression type: {messageExpression.GetType().Name}");
                            }
                        } else if (instruction.SupportsChildren && instruction.FirstChild != null) {
                            instructions.Enqueue(instruction.FirstChild);
                        }

                        if (instruction.Next != null) {
                            instructions.Enqueue(instruction.Next);
                        }
                    }

                    if (messages.Count == 0) {
                        Debug.Log("No Broadcast instructions found. This PhiloticParallax device will not be able to send any messages.");
                    }

                    foreach (var message in messages) {
                        Debug.Log($"Initializing transmitter for message '{message}'");
                        this._flightProgramScript.FlightProgram.RootInstructions.Add(
                            new PhiloticParallaxTransmitterInstruction(this, message)
                        );
                    }
                } else {
                    Debug.Log("Not InFlightScene");
                }
            }
        }

        protected override void OnInitialized() {
            base.OnInitialized();

            if (this.PartScript.Data.Activated && this._philoticParallaxEventListener == null) {
                // Pre-register
                Debug.Log($"Pre-Activating PhiloticParallax Listener {this.GetHashCode()}");
                PhiloticParallaxEvent += (this._philoticParallaxEventListener = PhiloticParallaxHandler);
            }
        }

        public override void OnActivated() {
            base.OnActivated();

            if (this._philoticParallaxEventListener == null) {
                // Pre-register
                Debug.Log($"Activating PhiloticParallax Listener {this.GetHashCode()}");
                PhiloticParallaxEvent += (this._philoticParallaxEventListener = PhiloticParallaxHandler);
            } else {
                Debug.Log($"Skipping Listener Activation {this.GetHashCode()}");
            }
        }

        public override void OnDeactivated() {
            Debug.Log("OnDeactivated");
            base.OnDeactivated();

            // Detach event handler
            if (this._philoticParallaxEventListener != null) {
                Debug.Log($"Deactivating PhiloticParallax Listener  {this.GetHashCode()}");
                PhiloticParallaxEvent -= this._philoticParallaxEventListener;
                this._philoticParallaxEventListener = null;
            }
        }

        protected override void OnDisposed() {
            Debug.Log($"PhiloticParallax {this.GetHashCode()} Disposed");
            base.OnDisposed();
            if (this._philoticParallaxEventListener != null) {
                PhiloticParallaxEvent -= this._philoticParallaxEventListener;
                this._philoticParallaxEventListener = null;
            }
        }

        private void PhiloticParallaxHandler(System.Object sender, PhiloticParallaxEventArgs args) {
            if (this.PartScript.Data.Activated && this._flightProgramScript != null && !ReferenceEquals(this, sender)) {
                this._flightProgramScript.BroadcastMessage(
                    true, // local
                    $"rx_{args.Message}",
                    args.Data
                );
            }
        }

        private void OnTransmitPhiloticParallaxMessage(String message, ExpressionResult data) {
            if (this.PartScript.Data.Activated) {
                if (this._battery != null) {
                    if (!this._battery.IsEmpty || this.Data.PowerConsumptionPerMessage <= 0) {
                        if (this.Data.PowerConsumptionPerMessage > 0) {
                            this._battery.RemoveFuel(
                                this.Data.PowerConsumptionPerMessage
                            );
                        }

                        Debug.Log($"Dispatching Philotic Parallax Message '{message}' to {PhiloticParallaxEvent?.GetInvocationList().Length ?? 0} listeners.");
                        PhiloticParallaxEvent?.Invoke(
                            this,
                            new PhiloticParallaxEventArgs(message, data.TextValue)
                        );
                    } else {
                        Debug.Log("Battery depleted. Unable to transmit Philotic Parallax message.");
                    }
                } else {
                    Debug.Log("Battery not found. Unable to transmit Philotic Parallax message.");
                }
            }
        }

        private class PhiloticParallaxTransmitterInstruction : EventInstruction {
            private static readonly FieldInfo _eventField =
                typeof(EventInstruction).GetField("_event", BindingFlags.Instance | BindingFlags.NonPublic);

            private readonly PhiloticParallaxScript _parent;
            private readonly string _message;

            public PhiloticParallaxTransmitterInstruction(
                PhiloticParallaxScript parent,
                String message) {

                this._parent = parent;
                // Drop the tx_ prefix
                this._message = message.Substring(3);
                Debug.Assert(_eventField != null, "ASSERT FAILED: _eventField != null");
                _eventField.SetValue(this, ProgramEventType.ReceiveMessage);
                this.InitializeExpressions(
                    new ConstantExpression(message)
                );
            }

            public override ProgramInstruction Execute(IThreadContext context) {
                this._parent.OnTransmitPhiloticParallaxMessage(this._message, context.GetLocalVariable("data")?.Value);
                return null;
            }
        }
    }

    public class PhiloticParallaxEventArgs {
        public String Message { get; }
        public String Data { get; }

        public PhiloticParallaxEventArgs(
            String message,
            String data) {
            this.Message = message;
            this.Data = data;
        }
    }
}
