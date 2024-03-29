#pragma once
#include "circuits/circuit_wire.hpp"
#include "circuits/circuit_io.hpp"
#include <vector>

class CircuitInput : public CircuitIO {
    private:
    CircuitWire* signal;

    public:
    CircuitInput(int bits, LogicValue initialValue) : CircuitIO(bits, initialValue) { signal = nullptr; }
    void SetSignal(CircuitWire* wire) { signal = wire; }
    CircuitWire* GetSignal() { return signal; }
    void GetValueFromSignal() {
        if (signal != nullptr) {
            this->SetValues(signal->GetValues());
        }
        else {
            SetValues(LogicValue_LOW);
        }
    }
    void RemoveSignal() { signal = nullptr; }
    bool HasSignal() { return signal != nullptr; }

    virtual ~CircuitInput() = default;
};