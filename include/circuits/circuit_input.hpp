#pragma once
#include "circuits/circuit_wire.hpp"
#include "circuits/circuit_io.hpp"

class CircuitInput : public CircuitIO {
private:
    CircuitWire* signal;

public:
    CircuitInput(LogicValue initialValue) : CircuitIO(initialValue) { signal = nullptr; }
    void SetSignal(CircuitWire* wire) { signal = wire; }
    CircuitWire* GetSignal() { return signal; }
    void GetValueFromSignal() {
        if (signal != nullptr) {
            this->SetValue(signal->GetValue());
        }
        else {
            SetValue(LogicValue_LOW);
        }
    }
    void RemoveSignal() { signal = nullptr; }
    bool HasSignal() { return signal != nullptr;}
};