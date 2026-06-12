import styled from 'styled-components';

const Panel = styled.div`
  background-color: var(--panel-bg);
  border-radius: 8px;
  padding: 20px;
  border: 1px solid var(--border-color);
  box-shadow: 0 2px 4px rgba(0,0,0,0.05);
`;

const PanelTitle = styled.div`
  font-size: 16px;
  font-weight: 700;
  margin-bottom: 15px;
  color: var(--primary-color);
  display: flex;
  align-items: center;
  gap: 8px;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 10px;
`;

const JogOuter = styled.div`
  display: flex;
  gap: 30px;
  justify-content: center;
  align-items: flex-start;
  background-color: var(--item-bg);
  padding: 20px;
  border-radius: 8px;
`;

const JogPad3Axis = styled.div`
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
  width: 250px;
`;

const JogPadRotary = styled.div`
  display: flex;
  flex-direction: column;
  gap: 12px;
`;

const RotaryRow = styled.div`
  display: flex;
  gap: 8px;
`;

const JogButton = styled.button<{ isRotary?: boolean }>`
  background: #2c3e50; /* Dark contrast */
  color: white;
  border: 1px solid #1a2533;
  width: ${props => props.isRotary ? '68px' : '75px'};
  height: ${props => props.isRotary ? '45px' : '50px'};
  border-radius: 6px;
  cursor: pointer;
  font-weight: 800;
  font-size: ${props => props.isRotary ? '15px' : '17px'};
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.1s ease;
  box-shadow: 0 2px 0 #1a2533;

  &:hover { background: #34495e; }
  &:active { 
    background: var(--accent-color); 
    transform: translateY(2px);
    box-shadow: none;
  }
`;

const SliderContainer = styled.div`
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
`;

const SpeedLabel = styled.label`
  font-size: 12px;
  color: var(--text-muted);
`;

const RangeInput = styled.input`
  width: 100%;
  cursor: pointer;
`;

const JogController = () => {
  return (
    <Panel>
      <PanelTitle>🎮 Manual Control (Jog)</PanelTitle>
      <JogOuter>
        <JogPad3Axis>
          <div />
          <JogButton>Z+</JogButton>
          <JogButton>Y+</JogButton>
          
          <JogButton>X-</JogButton>
          <div />
          <JogButton>X+</JogButton>
          
          <JogButton>Y-</JogButton>
          <JogButton>Z-</JogButton>
          <div />
        </JogPad3Axis>

        <JogPadRotary>
          <RotaryRow>
            <JogButton isRotary>A+</JogButton>
            <JogButton isRotary>A-</JogButton>
          </RotaryRow>
          <RotaryRow>
            <JogButton isRotary>B+</JogButton>
            <JogButton isRotary>B-</JogButton>
          </RotaryRow>
        </JogPadRotary>
      </JogOuter>

      <SliderContainer>
        <SpeedLabel>Jog Speed (Feedrate)</SpeedLabel>
        <RangeInput type="range" min="0" max="100" defaultValue="30" />
      </SliderContainer>
    </Panel>
  );
};

export default JogController;
