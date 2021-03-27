#pragma once

enum LogicValue {
    LogicValue_LOW = 0,
    LogicValue_HIGH = 1,
};

class CircuitIO {
    private:
    LogicValue value;

    public:
    CircuitIO(LogicValue initialValue) { value = initialValue; }
    LogicValue GetValue() { return value; }
    void SetValue(LogicValue val) { value = val; }
};