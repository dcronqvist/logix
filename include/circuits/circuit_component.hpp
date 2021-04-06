#pragma once

#include "circuits/circuit_input.hpp"
#include "circuits/circuit_output.hpp"
#include "circuits/circuit_io.hpp"
#include "circuits/circuit_wire.hpp"
#include <vector>

class CircuitComponent {
    public:
    std::vector<CircuitInput*> inputs;
    std::vector<CircuitOutput*> outputs;

    public:
    CircuitComponent(int inputAmount, int outputAmount) {
        for (int i = 0; i < inputAmount; i++) {
            inputs.push_back(new CircuitInput(LogicValue_LOW));
        }

        for (int i = 0; i < outputAmount; i++) {
            outputs.push_back(new CircuitOutput(LogicValue_LOW));
        }
    }

    void SetInputWire(int index, CircuitWire* wire) {
        inputs.at(index)->SetSignal(wire);
    }

    void RemoveOutputWire(int index, CircuitWire* wire) {
        CircuitOutput* output = outputs.at(index);
        for (int i = 0; i < output->GetSignals().size(); i++)
        {
            CircuitWire* ow = output->GetSignals().at(i);
            if(ow == wire) {
                output->RemoveOutputSignal(i);
                break;
            }
        }
    }

    void RemoveInputWire(int index) {
        this->inputs.at(index)->RemoveSignal();
    }

    void AddOutputWire(int index, CircuitWire* wire) {
        outputs.at(index)->AddOutputSignal(wire);
    }

    CircuitInput* GetInputFromIndex(int index) {
        return this->inputs.at(index);
    }

    CircuitOutput* GetOutputFromIndex(int index) {
        return this->outputs.at(index);
    }

    void UpdateInputs() {
        for (int i = 0; i < inputs.size(); i++) {
            CircuitInput* ci = inputs.at(i);
            ci->GetValueFromSignal();
        }
    }

    void UpdateOutputs() {
        for (int i = 0; i < outputs.size(); i++) {
            CircuitOutput* co = outputs.at(i);
            co->SetSignals();
        }
    }

    void UpdateInputsAndPerformLogic() {
        UpdateInputs();
        PerformLogic();
    }

    protected:
    virtual void PerformLogic() = 0;
};