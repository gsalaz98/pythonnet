from datetime import datetime, timedelta
from decimal import Decimal
from enum import Enum


class MarketDataType(Enum):
    Base = 0,
    TradeBar = 1,
    Tick = 2,
    Auxiliary = 3,
    QuoteBar = 4,
    OptionChain = 5,
    FuturesChain = 6


# noinspection PyPep8Naming
class BaseData:

    def __init__(self, dataType, isFillForward, time, endTime, symbol, value):
        self._DataType = dataType
        self._IsFillForward = isFillForward
        self._Time = time
        self._EndTime = endTime
        self._Symbol = symbol
        self._Value = value

        # if kwargs is None:
        #     self._DataType = MarketDataType.Base
        #     self._IsFillForward = False
        #     self._Time = datetime.min
        #     self._EndTime = datetime.min
        #     self._Symbol = None
        #     self._Value = Decimal(0)
        # else:
        #     if 'DataType' in kwargs:
        #         self._DataType = kwargs['DataType']
        #     else:
        #         self._DataType = MarketDataType.Base
        #     if 'IsFillForward' in kwargs:
        #         self._IsFillForward = kwargs['IsFillForward']
        #     else:
        #         self._IsFillForward = False
        #     if 'Time' in kwargs:
        #         self._Time = kwargs['Time']
        #     else:
        #         self._Time = datetime.min
        #     if 'EndTime' in kwargs:
        #         self._EndTime = kwargs['EndTime']
        #     else:
        #         self._EndTime = datetime.min
        #     if 'Symbol' in kwargs:
        #         self._Symbol = kwargs['Symbol']
        #     else:
        #         self._Symbol = None
        #     if 'Value' in kwargs:
        #         self._Value = kwargs['Value']
        #     else:
        #         self._Value = Decimal(0)

    @property
    def DataType(self):
        return self._DataType

    @DataType.setter
    def DataType(self, value):
        self._DataType = value

    @property
    def IsFillForward(self):
        return self._IsFillForward

    @IsFillForward.setter
    def IsFillForward(self, value):
        self._IsFillForward = value

    @property
    def Time(self):
        return self._Time

    @Time.setter
    def Time(self, value):
        self._Time = value

    @property
    def EndTime(self):
        return self._Time

    @EndTime.setter
    def EndTime(self, value):
        self._Time = value

    @property
    def Symbol(self):
        return self._Symbol

    @Symbol.setter
    def Symbol(self, value):
        self._Symbol = value

    @property
    def Value(self):
        return self._Value

    @Value.setter
    def Value(self, value):
        self._Value = value

    @property
    def Price(self):
        return self.Value()

    @Price.setter
    def Price(self, value):
        self.Value = value

    def ToString(self):
        return self.__repr__()

    def __repr__(self):
        return f"{self._Symbol}: {self._Value}"


# noinspection PyPep8Naming
class TradeBar(BaseData):

    def __init__(self, dataType, isFillForward, time, endTime, symbol, value, open, high, low):
        super().__init__(dataType, isFillForward, time, endTime, symbol, value)

        self._Open = open
        self._High = high
        self._Low = low

        # if kwargs is None:
        #     self._Open = Decimal(0)
        #     self._High = Decimal(0)
        #     self._Low = Decimal(0)
        #     self._Period = timedelta(0)
        # else:
        #     if 'Open' in kwargs:
        #         self._Open = kwargs['Open']
        #     else:
        #         self._Open = Decimal(0)
        #     if 'High' in kwargs:
        #         self._High = kwargs['High']
        #     else:
        #         self._High = Decimal(0)
        #     if 'Low' in kwargs:
        #         self._Low = kwargs['Low']
        #     else:
        #         self._Low = Decimal(0)
        #     if 'Period' in kwargs:
        #         self._Period = kwargs['Period']
        #     else:
        #         self._Period = timedelta(0)
        #     if 'Close' in kwargs:
        #         self.Value = kwargs['Close']

    @property
    def EndTime(self):
        return super().EndTime()

    @EndTime.setter
    def EndTime(self, value):
        self._Period = self._Time - value

    @property
    def Open(self):
        return self._Open

    @Open.setter
    def Open(self, value):
        self._Open = value

    @property
    def High(self):
        return self._High

    @High.setter
    def High(self, value):
        self._High = value

    @property
    def Low(self):
        return self._Low

    @Low.setter
    def Low(self, value):
        self._Low = value

    @property
    def Close(self):
        return self._Value

    @Close.setter
    def Close(self, value):
        self.Value = value

    @property
    def Period(self):
        return self._Period

    @Period.setter
    def Period(self, value):
        self._Period = value

#
# bar = TradeBar(1, False, datetime(2000,1,2,3,4,5,6), datetime(2000,2,3,4,5,6,7), None, 1, 2, 3, 4)
#
# r = repr(bar)
#
# print(r)
